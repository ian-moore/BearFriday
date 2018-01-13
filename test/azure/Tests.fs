namespace BearFriday.Azure.Test

open System
open FsUnit.Xunit
open Xunit


module ``ExtractSeedDataToQueue Tests`` =
    open BearFriday.Azure.ExtractSeedDataToQueue

    [<Fact>]
    let ``url with no subdomain should return the id`` () =
        let id = "BXqGOTRgbyc"
        let url = sprintf "https://instagram.com/p/%s/" id
        match getIdFromShareUrl url with 
        | Some v -> v |> should equal id
        | _ -> failwith "Did not match id."
        ()

    [<Fact>]
    let ``url with subdomain should return the id`` () =
        let id = "Bd3KehEj9jn"
        let url = sprintf "https://www.instagram.com/p/%s/" id
        match getIdFromShareUrl url with
        | Some v -> v |> should equal id
        | _ -> failwith "Did not match id."
        ()

    [<Fact>]
    let ``url with query params should return the id`` () =
        let id = "BdyiMNdghsx"
        let url = sprintf "https://instagram.com/p/%s/?saved-by=ian.m.moore" id
        match getIdFromShareUrl url with
        | Some v -> v |> should equal id
        | _ -> failwith "Did not match id."
        ()
    
    [<Fact>]
    let ``url with no id returns None`` () =
        let url = "http://fsharp.org/learn.html"
        match getIdFromShareUrl url with 
        | Some v -> failwithf "Found invalid id %s." v
        | _ -> ()
