namespace ShabbBlog.Pagination

//-- JSON Serialization
open System.Runtime.Serialization
open System.Runtime.Serialization.Json

//-- DateTime
open System

open ShabbBlog.Entry

module Pagination =
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

