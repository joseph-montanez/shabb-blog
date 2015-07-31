module ShabbBlog

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
    type Entry = {
        [<field : DataMember(Name="ID")>]
        ID : string
        [<field : DataMember(Name="URL")>]
        URL : string
        [<field : DataMember(Name="Title")>]
        Title : string
        [<field : DataMember(Name="PubDate")>]
        PubDate : DateTime
        [<field : DataMember(Name="Content")>]
        Content : string
        [<field : DataMember(Name="Published")>]
        Published : bool
    }
    [<DataContract>]
    type Page = {
        [<field : DataMember(Name="Items")>]
        Items : Entry[]
        [<field : DataMember(Name="NextPage")>]
        NextPage : bool
        [<field : DataMember(Name="PrevPage")>]
        PrevPage : bool
    }
    [<DataContract>]
    type Tag = {
        [<field : DataMember(Name="Name")>]
        Name: string
        [<field : DataMember(Name="Slug")>]
        Slug: string
    }
    let processDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)
    let xmlFilename = processDir + "/data/blog-4fd782ad.xml"
    let ParseDate date = System.DateTime.Parse date
    let xn a = XName.Get(a)
    let doc = XDocument.Load(xmlFilename)
    let contentNS = XNamespace.Get "http://purl.org/rss/1.0/modules/content/"
    let wordpressNS = XNamespace.Get "http://wordpress.org/export/1.0/"
    let tags = [
        for el in doc.Descendants(wordpressNS + "tag") ->
            {
                Name = el.Element(wordpressNS + "tag_name").Value
                Slug = el.Element(wordpressNS + "tag_slug").Value
            }
    ]
    let items = [
        for el in doc.Descendants(xn "item") ->
            {
                ID = el.Element(wordpressNS + "post_id").Value
                URL = el.Element(wordpressNS + "post_name").Value
                Title = el.Element(xn "title").Value
                PubDate = el.Element(xn "pubDate").Value |> ParseDate
                Content = el.Element(contentNS + "encoded").Value
                Published = if String.Compare(el.Element(wordpressNS + "status").Value, "publish") > -1 then true else false
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
        );
        path "/blog/posts/pages/count" >>= context(fun _ -> OK <| (Blog.CountPages 5).ToString())
        path "/blog/tags/all" >>= context(fun _ -> Blog.tags |> Seq.toArray |> OKJsonBytes)
    ]


choose [
    GET >>= path "/" >>= GetIndex;
    GET >>= path "/static" >>= GetStatic;
    //GET >>= pathScan "/api/blog/page/%d" JsonResponse.GetCar;
    testapp
    blog
    NOT_FOUND "Found no handlers"
] |> startWebServer cfg
