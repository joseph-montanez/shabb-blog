namespace Pagination

//-- JSON Serialization
open System.Runtime.Serialization
open System.Runtime.Serialization.Json

//-- DateTime
open System

module Pagination =
    [<DataContract>]
    type PagePreview = {
        [<field : DataMember(Name="ID")>]
        ID : int
        [<field : DataMember(Name="PubDate")>]
        PubDate : DateTime
        [<field : DataMember(Name="Title")>]
        Title : string
        [<field : DataMember(Name="Slug")>]
        Slug : string
    }

