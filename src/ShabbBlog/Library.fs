module ShabbBlog

open XmlTools
open Tag
open Pager
open Entry
open Pagination

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

//open FSharp.Data

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
open Suave.Http.Writers
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
    let processDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)
    let xmlFilename = processDir + "/data/blog-4fd782ad.xml"
    let ParseDate date = System.DateTime.Parse date
    let doc = Xml.Load xmlFilename
    let tags = Tag.parse doc
    let items : Entry.Entry list = Entry.Parse doc

    let sample = "blog-4fd782ad.xml"

    let GetPushblishedPosts (items : Entry.Entry list) =
        List.filter (fun (entry : Entry.Entry) -> entry.Published) items

    let SortPostsByDate (items : Entry.Entry list) =
        List.sortBy (fun (entry : Entry.Entry) -> -entry.PubDate.ToBinary()) items

    let GetSortedPushblishedPosts items =
        items
            |> GetPushblishedPosts
            |> SortPostsByDate

    let publishedPosts = items |> GetSortedPushblishedPosts

    let GetPost items index = items[index]

    let HasNextPage perPage pageNo = perPage * pageNo < publishedPosts.Length
    let HasPrevPage pageNo = pageNo > 1

    let CountPages perPage =
        let pages : double = (double publishedPosts.Length) / (double perPage)
        (int (Math.Ceiling pages))

    let GetPage perPage pageNo =
        let startOf = Math.Max(0, perPage * (pageNo - 1))
        let endOf = Math.Min(publishedPosts.Length - 1, startOf + perPage)

        (publishedPosts |> Seq.toArray).[startOf..endOf]

    let EntryToPagePreview (entry : Entry.Entry) =
        let item : Pagination.PagePreview = {
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
        bindings = [ HttpBinding.mk' HTTP "0.0.0.0" 8083 ]
}

let allow_cors : WebPart =
    choose [
        OPTIONS 
            >>= setHeader "Access-Control-Allow-Origin" "*" 
            >>= setHeader "Access-Control-Allow-Methods" "POST, GET, OPTIONS" 
            >>= setHeader "Access-Control-Allow-Headers" "X-Requested-With, Origin, Content-Type, Access-Control-Allow-Origin" 
            >>= OK ""
    ]


let testapp : WebPart =
  choose
    [
        pathScan "/add/%d/%d" (fun (a,b) -> OK((a + b).ToString()))
    ]

let blog : WebPart =
  choose
    [
        GET >>= setHeader "Access-Control-Allow-Origin" "*" >>= pathScan "/blog/posts/page/%d" (fun (pageNo) ->
            let entryToExcerpt (entry:Entry.Entry.Entry) = 
                let excerpt : Entry.Excerpt = { 
                    ID = entry.ID
                    Slug = entry.Slug
                    Title = entry.Title
                    PubDate = entry.PubDate
                }
                excerpt
            let page : Page.PageIndex = {
                Items = Blog.GetPage 5 pageNo |> Array.map entryToExcerpt
                NextPage = Blog.HasNextPage 5 pageNo
                PrevPage = Blog.HasPrevPage pageNo
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
    allow_cors
    GET >>= path "/" >>= setHeader "ABC" "123" >>= OK "YES!"
    GET >>= path "/static" >>= GetStatic;
    //GET >>= pathScan "/api/blog/page/%d" JsonResponse.GetCar;
    testapp
    blog
    NOT_FOUND "Found no handlers"
] |> startWebServer cfg
