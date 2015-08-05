namespace ShabbBlog.Pager

//-- JSON Serialization
open System.Runtime.Serialization
open System.Runtime.Serialization.Json

open ShabbBlog.Entry.Entry

module Page =
    [<DataContract>]
    type PageIndex = {
        [<field : DataMember(Name="Items")>]
        Items : Entry []
        [<field : DataMember(Name="NextPage")>]
        NextPage : bool
        [<field : DataMember(Name="PrevPage")>]
        PrevPage : bool
    }

