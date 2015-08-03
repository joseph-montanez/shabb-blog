module ShabbBlog

open ShabbBlog.Xml
open ShabbBlog.Tag
open ShabbBlog.Entry

//-- Log Requests to Console
open System

//-- Safe Immutable Data
open System.Collections.Concurrent

//-- XML for Blog
open System.Xml
open System.Xml.Linq

//-- JSON Serialization
open System.Runtime.Serialization
open System.Runtime.Serialization.Json
open System.IO
open System.Diagnostics
open System.Text

//-- Suave Web Server
open Suave
open Suave.Logging
open Suave.Web
open Suave.Http
open Suave.Types
open Suave.Http.Successful
open Suave.Http.ServerErrors
open Suave.Http.RequestErrors
open Suave.Http.Applicatives
open Suave.Http.Response
open Suave.Http.Files
open Suave.Utils
open Suave.Json

/// Documentation for my library
///
/// ## Example
///
///     let h = Library.hello 1
///     printfn "%d" h
///
module Blog = 
    /// Returns 42
    ///
    /// ## Parameters
    ///  - `num` - whatever
    let hello num = 42
    [<DataContract>]
    type PagePreview = {
        ID : int
        PubDate : DateTime
        Title : string
        Slug : string
    }
    [<DataContract>]
    type Page = {
        [<field : DataMember(Name="Items")>]
        Items : Entry.Entry[]
        [<field : DataMember(Name="NextPage")>]
        NextPage : bool
        [<field : DataMember(Name="PrevPage")>]
        PrevPage : bool
    }
    let processDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)
    let xmlFilename = processDir + "/data/blog-4fd782ad.xml"
    let ParseDate date = System.DateTime.Parse date
    let doc = Xml.load xmlFilename
    let tags = Tag.parse doc
    let items = [
        for el in doc.Descendants(Xml.xn "item") ->
            {
                ID = el.Element(Xml.wordpressNS + "post_id").Value |> int
                Slug = el.Element(Xml.wordpressNS + "post_name").Value
                Title = el.Element(Xml.xn "title").Value
                PubDate = el.Element(Xml.xn "pubDate").Value |> ParseDate
                Content = el.Element(Xml.contentNS + "encoded").Value
                Published = if String.Compare(el.Element(Xml.wordpressNS + "status").Value, "publish") > -1 then true else false
                NextPage = None
                PrevPage = None
                Categories = [|for categoryEl in el.Elements(Xml.xn "category") -> categoryEl.Value|] 
            }
    ]

    let GetPushblishedPosts items = 
        List.filter (fun entry -> entry.Published) items

    let SortPostsByDate items =
        List.sortBy (fun entry -> -entry.PubDate.ToBinary()) items

    let GetSortedPushblishedPosts items =
        items 
            |> GetPushblishedPosts
            |> SortPostsByDate

    let publishedPosts = items |> GetSortedPushblishedPosts

    let GetPost items index = items[index]
    
    let HasNextPage perPage pageNo = perPage * pageNo < publishedPosts.Length
    let HasPrevPage perPage pageNo = perPage * pageNo > 1
    
    let CountPages perPage = 
        let pages : double = (double publishedPosts.Length) / (double perPage)
        (int (Math.Ceiling pages))

    let GetPage perPage pageNo = 
        let startOf = Math.Max(0, perPage * (pageNo - 1))
        let endOf = Math.Min(publishedPosts.Length - 1, startOf + perPage)
        
        (publishedPosts |> Seq.toArray).[startOf..endOf]

    let EntryToPagePreview entry = 
        let item : PagePreview = {
            ID = entry.ID
            PubDate = entry.PubDate
            Title = entry.Title
            Slug = entry.Slug
        }
        item




let GetIndex = browseFileHome (Blog.processDir + "/index.html")
let GetStatic = browse Blog.processDir
let OKJsonBytes data = data |> toJson |> System.Text.Encoding.UTF8.GetString |> OK

let logger = Loggers.ConsoleWindowLogger LogLevel.Error
let cfg = {
    defaultConfig with
        logger = logger
}

let testapp : WebPart =
  choose
    [
        pathScan "/add/%d/%d" (fun (a,b) -> OK((a + b).ToString()))
    ]

let blog : WebPart =
  choose
    [
        pathScan "/blog/posts/page/%d" (fun (pageNo) -> 
            let page : Blog.Page = {
                Items = Blog.GetPage 5 pageNo 
                NextPage = Blog.HasNextPage 5 pageNo
                PrevPage = Blog.HasPrevPage 5 pageNo
            }
            page |> OKJsonBytes
        )
        pathScan "/blog/posts/entry/%d" (fun (entryId) ->
            // TODO: what if there is no entry found? first check if it exists! 
            let foundIndex = Blog.items |> Seq.findIndex (fun item -> entryId |> item.ID.Equals)
            let foundItem = Blog.items.[foundIndex]
            let nextItem = Blog.items.[foundIndex + 1]
            let prevItem = Blog.items.[foundIndex - 1]
            // TODO: there may not be a next or previous
            let nextPage = Blog.EntryToPagePreview nextItem
            let prevPage = Blog.EntryToPagePreview prevItem
            let item = {
                foundItem with
                    NextPage = Some(nextPage)
                    PrevPage = Some(prevPage)
            }
            OKJsonBytes(item)
        )
        path "/blog/posts/pages/count" >>= (OK <| (Blog.CountPages 5).ToString())
        path "/blog/tags/all" >>= (Blog.tags |> Seq.toArray |> OKJsonBytes)
    ]


choose [
    GET >>= path "/" >>= GetIndex;
    GET >>= path "/static" >>= GetStatic;
    //GET >>= pathScan "/api/blog/page/%d" JsonResponse.GetCar;
    testapp
    blog
    NOT_FOUND "Found no handlers"
] |> startWebServer cfg
