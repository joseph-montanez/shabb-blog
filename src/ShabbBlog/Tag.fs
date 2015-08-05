namespace Tag

open XmlTools

//-- XML for Parsing
open System.Xml
open System.Xml.Linq

//-- JSON Serialization
open System.Runtime.Serialization
open System.Runtime.Serialization.Json
open System.IO

module Tag =
    [<DataContract>]
    type Tag = {
        [<field : DataMember(Name="Name")>]
        Name: string
        [<field : DataMember(Name="Slug")>]
        Slug: string
    }
    
    let parse (doc : XDocument) = [
        for el in doc.Descendants(Xml.wordpressNS + "tag") ->
            {
                Name = el.Element(Xml.wordpressNS + "tag_name").Value
                Slug = el.Element(Xml.wordpressNS + "tag_slug").Value
            }
    ]