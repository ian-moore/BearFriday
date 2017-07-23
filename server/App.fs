module BearFriday.Server.App

open Giraffe.HttpHandlers
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open System

type AppSettings = 
    { EnableFriday: bool
      AzureConnection: string
      AzureTableName: string }

let currentDayOfWeek () = 
    let d = TimeSpan (-5, 0, 0) |> DateTimeOffset.UtcNow.ToOffset
    d.DayOfWeek

let dayIsFriday getDay ctx = async {
        match getDay () with
        | DayOfWeek.Friday -> return Some ctx
        | _ -> return None
    }
    
let todayIsFriday : HttpHandler = dayIsFriday currentDayOfWeek

let enableFeature toggle ctx = async {
        match toggle with
        | true -> return Some ctx
        | false -> return None
    }

let errorHandler (ex: Exception) (logger: ILogger) (ctx: HttpContext) =
    logger.LogError(EventId(0), ex, "An unhandled exception has occurred while executing the request.")
    ctx |> (clearResponse >=> setStatusCode 500 >=> text ex.Message)

let createApp config : HttpHandler = 
    let showBearFriday = choose [ todayIsFriday ; enableFeature config.EnableFriday ] 

    choose [
        route "/" >=> choose [
            showBearFriday >=> text "it's friday!"
            text "it's not friday yet"
        ]
        setStatusCode 404 >=> text "Not Found" 
    ]