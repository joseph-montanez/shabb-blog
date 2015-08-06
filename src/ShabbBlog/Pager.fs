namespace Pager

//-- Used for Math
open System

//-- JSON Serialization
open System.Runtime.Serialization
open System.Runtime.Serialization.Json

open Post
open Pagination

module Page =
    [<DataContract>]
    type PageIndex = {
        [<field : DataMember(Name="Items")>]
        Items : Post.Excerpt []
        [<field : DataMember(Name="NextPage")>]
        NextPage : bool
        [<field : DataMember(Name="PrevPage")>]
        PrevPage : bool
    }
    let HasNextPage postsLength perPage pageNo = perPage * pageNo < postsLength
    let HasPrevPage pageNo = pageNo > 1
    
    let GeneratePage  (entries:Post.Post.Post[]) perPage pageNo =
        let startOf = Math.Max(0, perPage * (pageNo - 1))
        let endOf = Math.Min(entries.Length - 1, startOf + perPage)
        (entries |> Seq.toArray).[startOf..endOf]

    let GetPage (entries:Post.Post.Post[]) (pageNo:int) = {
        Items = GeneratePage entries 5 pageNo |> Array.map Post.PostToExcerpt
        NextPage = HasNextPage entries.Length 5 pageNo
        PrevPage = HasPrevPage pageNo
    }

    let EntryToPagePreview (optionalPost : Post.Post option) =
        match optionalPost with
        | Some post ->
            Some({
                    Pagination.PagePreview.ID = post.ID
                    Pagination.PagePreview.PubDate = post.PubDate
                    Pagination.PagePreview.Title = post.Title
                    Pagination.PagePreview.Slug = post.Slug
            })
        | None -> None

    let GetEntry (entries:Post.Post.Post[]) (entriesLength:int) (entryId:int) =
        // TODO: what if there is no entry found? first check if it exists!
        let foundIndex = entries |> Seq.findIndex (fun item -> entryId |> item.ID.Equals)
        let foundItem =entries.[foundIndex]

        let nextItem : Post.Post.Post option = 
            let nextIndex = foundIndex + 1
            if nextIndex < entriesLength then
                Some(entries.[nextIndex])
            else
                None

        let prevItem : Post.Post.Post option = 
            let prevIndex = foundIndex - 1
            if prevIndex >= 0 then
                Some(entries.[prevIndex])
            else
                None

        let nextPage = EntryToPagePreview nextItem
        let prevPage = EntryToPagePreview prevItem
        {
            foundItem with
                NextPage = nextPage
                PrevPage = prevPage
        }