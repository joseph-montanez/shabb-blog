module ShabbBlog

open XmlTools
open Tag
open Pager
open Post
open Pagination
open Contact

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
    let items : Post.Post[] = Post.Parse doc |> List.toArray

    let sample = "blog-4fd782ad.xml"

    //-- TODO: all these functions need to be refactored to the post library
    let GetPushblishedPosts (items : Post.Post []) =
        Array.filter (fun (post : Post.Post) -> post.Published) items

    let SortPostsByDate (items : Post.Post []) =
        Array.sortBy (fun (post : Post.Post) -> -post.PubDate.ToBinary()) items

    let GetSortedPushblishedPosts items =
        items
            |> GetPushblishedPosts
            |> SortPostsByDate
            
    let publishedPosts = items |> GetSortedPushblishedPosts
    let publishedPostsLength = Array.length publishedPosts

    let GetPost items index = items[index]

    let HasNextPage perPage pageNo = perPage * pageNo < publishedPosts.Length
    let HasPrevPage pageNo = pageNo > 1

    let CountPages perPage =
        let pages : double = (double publishedPosts.Length) / (double perPage)
        (int (Math.Ceiling pages))



let GetPublishedPostsPage = Pager.Page.GetPage Blog.publishedPosts
let GetPublishedPostsEntry = Pager.Page.GetEntry Blog.publishedPosts Blog.publishedPostsLength


let GetIndex = browseFileHome (Blog.processDir + "/index.html")
let GetStatic = browse Blog.processDir
let OKJsonBytes data = data |> toJson |> System.Text.Encoding.UTF8.GetString |> OK

let logger = Loggers.ConsoleWindowLogger LogLevel.Error
let cfg = {
    defaultConfig with
        logger = logger
        bindings = [ HttpBinding.mk' HTTP "0.0.0.0" 8083 ]
}

let corsHeader : WebPart =
        setHeader "Access-Control-Allow-Origin" "*" 
let corsHeaderFull : WebPart =
        setHeader "Access-Control-Allow-Origin" "*" 
        >>= setHeader "Access-Control-Allow-Methods" "POST, GET, OPTIONS" 
        >>= setHeader "Access-Control-Allow-Headers" "X-Requested-With, Origin, Content-Type, Access-Control-Allow-Origin" 

let allowCors : WebPart = choose [OPTIONS >>= corsHeaderFull >>= OK ""]


let testapp : WebPart =
  choose
    [
        pathScan "/add/%d/%d" (fun (a,b) -> OK((a + b).ToString()))
    ]

let contact : WebPart =
    corsHeader >>= choose [
        POST  >>= path "/contact" >>= mapJson (fun (data:Contact.ContactData) ->
            printfn "email is '%s'" data.email
            let nothign = Environment.GetEnvironmentVariable "PATH"
            printfn "%s" nothign
            data
        )
    ]

let blog : WebPart =
  corsHeader >>= choose
    [
        pathScan "/blog/posts/page/%d" (fun (pageNo) -> GetPublishedPostsPage pageNo |> OKJsonBytes)
        pathScan "/blog/posts/entry/%d" (fun (entryId) -> GetPublishedPostsEntry entryId |> OKJsonBytes)
        path "/blog/posts/pages/count" >>= (OK <| (Blog.CountPages 5).ToString())
        path "/blog/tags/all" >>= (Blog.tags |> Seq.toArray |> OKJsonBytes)
    ]


choose [
    allowCors
    GET >>= path "/" >>= setHeader "ABC" "123" >>= OK "YES!"
    GET >>= path "/static" >>= GetStatic;
    //GET >>= pathScan "/api/blog/page/%d" JsonResponse.GetCar;
    testapp
    blog
    contact
    NOT_FOUND "Found no handlers"
] |> startWebServer cfg
