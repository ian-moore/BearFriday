module BearFriday.Server.Handlers

open Giraffe.HttpHandlers
open Giraffe.Razor.HttpHandlers
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open System
open server2.Models

let errorHandler (ex: Exception) (logger: ILogger) (ctx: HttpContext) =
    logger.LogError(EventId(0), ex, "An unhandled exception has occurred while executing the request.")
    ctx |> (clearResponse >=> setStatusCode 500 >=> text ex.Message)


let webApp : HttpHandler = 
    choose [
        GET >=>
            choose [
                route "/" >=> razorHtmlView "Index" { Text = "Hello world, from Giraffe!" }
            ]
        setStatusCode 404 >=> text "Not Found" 
    ]