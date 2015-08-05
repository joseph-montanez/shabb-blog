namespace Pager

//-- JSON Serialization
open System.Runtime.Serialization
open System.Runtime.Serialization.Json

open Entry

module Page =
    [<DataContract>]
    type PageIndex = {
        [<field : DataMember(Name="Items")>]
        Items : Entry.Excerpt []
        [<field : DataMember(Name="NextPage")>]
        NextPage : bool
        [<field : DataMember(Name="PrevPage")>]
        PrevPage : bool
    }

