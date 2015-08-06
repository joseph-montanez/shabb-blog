namespace Post

open System

//-- XML for Parsing
open System.Xml
open System.Xml.Linq

//-- JSON Serialization
open Microsoft.FSharp.Reflection 
open System.Reflection 
open System.Runtime.Serialization
open System.Runtime.Serialization.Json
open System.IO

open XmlTools
open Utils
open Pagination


module Post =
    [<DataContract>]
    type Post = {
        [<field : DataMember(Name="ID")>]
        ID : int
        [<field : DataMember(Name="Slug")>]
        Slug : string
        [<field : DataMember(Name="Title")>]
        Title : string
        [<field : DataMember(Name="PubDate")>]
        PubDate : DateTime
        [<field : DataMember(Name="Content")>]
        Content : string
        [<field : DataMember(Name="Published")>]
        Published : bool
        [<field : DataMember(Name="NextPage")>]
        NextPage : Pagination.PagePreview option
        [<field : DataMember(Name="PrevPage")>]
        PrevPage : Pagination.PagePreview option
        [<field : DataMember(Name="Categories")>]
        Categories : string []
    }

    [<DataContract>]
    type Excerpt = {
        [<field : DataMember(Name="ID")>]
        ID : int
        [<field : DataMember(Name="Slug")>]
        Slug : string
        [<field : DataMember(Name="Title")>]
        Title : string
        [<field : DataMember(Name="PubDate")>]
        PubDate : DateTime
    }

    let Parse (doc : XDocument) = [
        for el in doc.Descendants(Xml.XN "item") ->
            {
                ID = el.Element(Xml.wordpressNS + "post_id").Value |> int
                Slug = el.Element(Xml.wordpressNS + "post_name").Value
                Title = el.Element(Xml.XN "title").Value
                PubDate = el.Element(Xml.XN "pubDate").Value |> Utils.ParseDate
                Content = el.Element(Xml.contentNS + "encoded").Value
                Published = if String.Compare(el.Element(Xml.wordpressNS + "status").Value, "publish") > -1 then true else false
                NextPage = None
                PrevPage = None
                Categories = [|for categoryEl in el.Elements(Xml.XN "category") ->categoryEl.Value|]
            }
    ]

    
    let PostToExcerpt (post:Post) = 
        { 
            ID = post.ID
            Slug = post.Slug
            Title = post.Title
            PubDate = post.PubDate
        }
