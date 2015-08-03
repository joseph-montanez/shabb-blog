namespace ShabbBlog.Xml

//-- XML for Parsing
open System.Xml
open System.Xml.Linq

module Xml =
    let XN a = XName.Get(a)
    let Load (uri:string) = XDocument.Load(uri)
    let contentNS = XNamespace.Get "http://purl.org/rss/1.0/modules/content/"
    let wordpressNS = XNamespace.Get "http://wordpress.org/export/1.0/"

