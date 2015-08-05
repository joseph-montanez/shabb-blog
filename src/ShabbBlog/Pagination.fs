namespace ShabbBlog.Pagination

//-- JSON Serialization
open System.Runtime.Serialization
open System.Runtime.Serialization.Json

//-- DateTime
open System

module Pagination =
    [<DataContract>]
    type PagePreview = {
        ID : int
        PubDate : DateTime
        Title : string
        Slug : string
    }

