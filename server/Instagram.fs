module BearFriday.Server.Instagram

open System.Text.RegularExpressions
    

let getIdFromShareUrl url =
    let m = Regex.Match(url, "https?://instagram.com/p/([\w]+)/")
    match m.Success with
    | true -> Some m.Groups.[1].Value
    | false -> None