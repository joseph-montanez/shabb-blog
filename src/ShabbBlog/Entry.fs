namespace ShabbBlog.Entry

open System

//-- XML for Parsing
open System.Xml
open System.Xml.Linq

//-- JSON Serialization
open System.Runtime.Serialization
open System.Runtime.Serialization.Json
open System.IO

open ShabbBlog.Xml


module Entry =
    [<DataContract>]
    type Entry = {
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
        NextPage : PagePreview option
        [<field : DataMember(Name="PrevPage")>]
        PrevPage : PagePreview option
        [<field : DataMember(Name="Categories")>]
        Categories : string []
    }
    
    let parse (doc : XDocument) = [
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