module BearFriday.Server.Test.Instagram

open BearFriday.Server
open System
open Xunit

[<Fact>]
let ``idFromShareUrl returns id`` () =
    let url = "https://instagram.com/p/BXG5JVpgnh9/"
    let expected = "BXG5JVpgnh9"
    let actual = Instagram.getIdFromShareUrl url
    
    match actual with
    | Some a -> Assert.Equal(expected, a)
    | None -> Assert.True(false)
    
