module BearFriday.Server.App

open Giraffe
open Giraffe.HttpHandlers
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open System

type AppSettings = 
    { EnableFriday: bool
      AzureConnection: string
      AzureTableName: string
      InstagramClientId: string
      InstagramClientSecret: string
      InstagramRedirectUri: string }

let currentDayOfWeek () = 
    let d = TimeSpan (-5, 0, 0) |> DateTimeOffset.UtcNow.ToOffset
    d.DayOfWeek

let dayIsFriday getDay ctx = async {
        match getDay () with
        | DayOfWeek.Friday -> return Some ctx
        | _ -> return None
    }
    
let todayIsFriday = dayIsFriday currentDayOfWeek

let enableFeature toggle ctx = async {
        match toggle with
        | true -> return Some ctx
        | false -> return None
    }

let requestInstagramToken config = 
    Instagram.requestAccessToken config.InstagramClientId config.InstagramClientSecret config.InstagramRedirectUri

let errorHandler (ex: Exception) (logger: ILogger) (ctx: HttpContext) =
    logger.LogError(EventId(0), ex, "An unhandled exception has occurred while executing the request.")
    ctx |> (clearResponse >=> setStatusCode 500 >=> text ex.Message)

let createApp config : HttpHandler = 
    let showBearFriday = choose [ todayIsFriday ; enableFeature config.EnableFriday ] 
    let loginUrl = Instagram.buildAuthUrl config.InstagramClientId config.InstagramRedirectUri
    let getToken = requestInstagramToken config
    
    choose [
        route "/" >=> choose [
            showBearFriday >=> text "it's friday!"
            text "it's not friday yet"
        ]
        route "/login" >=> redirectTo false loginUrl
        route "/oauth" >=> warbler (fun ctx -> 
            match (ctx.GetQueryStringValue "code", ctx.GetQueryStringValue "error") with
            | Ok c, _ -> text c
            | Error _, Ok err -> setStatusCode 400
            | _, _ -> setStatusCode 400
        )
        setStatusCode 404 >=> text "Not Found" 
    ]