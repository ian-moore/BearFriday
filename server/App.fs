module BearFriday.App

open System
open Suave
open Suave.Operators
open Suave.Filters

type AppConfiguration = { 
    EnableFriday: bool; 
    EnableTuesday: bool;
    InstagramClientId: string;
    InstagramClientSecret: string;
    InstagramRedirectUri: string;
}

let getCurrentDayOfWeek () = 
    let d = TimeSpan (-5, 0, 0) |> DateTimeOffset.UtcNow.ToOffset
    d.DayOfWeek

let dayIsFriday getDay ctx = 
    async {
        match getDay () with
        | DayOfWeek.Friday -> return Some ctx
        | _ -> return None
    }
    
let dayIsTuesday getDay ctx =
    async {
        match getDay () with
        | DayOfWeek.Tuesday -> return Some ctx
        | _ -> return None
    }

let todayIsTuesday : WebPart = dayIsTuesday getCurrentDayOfWeek

let todayIsFriday : WebPart = dayIsFriday getCurrentDayOfWeek

let enableFeature toggle ctx =
    async {
        match toggle with
        | true -> return Some ctx
        | false -> return None
    }

let createApp config = 
    let showBearFriday = enableFeature config.EnableFriday >=> todayIsFriday
    let showSquirrelTuesday = enableFeature config.EnableTuesday >=> todayIsTuesday
    let loginUrl = Instagram.buildAuthUrl config.InstagramClientId config.InstagramRedirectUri

    choose [
        path "/" >=> choose [
            showBearFriday >=> context (fun c -> Successful.OK "it's friday!")
            showSquirrelTuesday >=> context (fun c -> Successful.OK "it's tuesday!")
            //Files.file "views/friday.html"
            Successful.OK "it's a normal day"
        ]
        path "/login" >=> Redirection.FOUND loginUrl
        path "/oauth" >=> request (fun req ->
            match req.queryParam "code" with
            | Choice1Of2 c -> Successful.OK "thanks"
            | Choice2Of2 x -> RequestErrors.BAD_REQUEST x
        )
        path "/curate" >=> Successful.OK "curator page"
        pathRegex "(.*)\.(css|png|gif|js|map|ico)" >=> Files.browseHome
        RequestErrors.NOT_FOUND "Not found"
    ]
