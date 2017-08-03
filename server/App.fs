module BearFriday.Server.App

open BearFriday.Server.Storage
open Giraffe
open Giraffe.HttpHandlers
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open System
open System.Security.Claims

type AppSettings = 
    { EnableFriday: bool
      AzureConnectionString: string
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

let storeToken (storage: StorageClient) (token: Instagram.Token) =
    { Id = token.User.Id
      Username = token.User.Username
      AccessToken = token.AccessToken } 
    |> storage.StoreUser

let authScheme = "Cookie"

let signIn (userId, username) =
    fun (ctx: HttpContext) -> async {
        let issuer = "http://localhost:5000"
        let claims = 
            [ Claim(ClaimTypes.Name, username, ClaimValueTypes.String, issuer)
              Claim(ClaimTypes.NameIdentifier, userId, ClaimValueTypes.String, issuer) ]
        let user = ClaimsIdentity(claims, authScheme) |> ClaimsPrincipal
        do! ctx.Authentication.SignInAsync(authScheme, user) |> Async.AwaitTask
        return Some ctx
    }

let handleAsyncResult okFunc errorFunc (ar: Async<Result<'a,'b>>) =
    fun (ctx: HttpContext) -> async {
        let! r = ar
        match r with 
        | Ok v -> return! okFunc v ctx
        | Error ex -> return! errorFunc ex ctx
    }



let errorHandler (ex: Exception) (logger: ILogger) (ctx: HttpContext) =
    logger.LogError(EventId(0), ex, "An unhandled exception has occurred while executing the request.")
    ctx |> (clearResponse >=> setStatusCode 500 >=> text ex.Message)

let createApp config = 
    let showBearFriday = choose [ todayIsFriday ; enableFeature config.EnableFriday ] 
    let loginUrl = Instagram.buildAuthUrl config.InstagramClientId config.InstagramRedirectUri
    let getToken = requestInstagramToken config
    let storage = StorageClient (config.AzureConnectionString, config.AzureTableName)
    
    choose [
        route "/" >=> choose [
            showBearFriday >=> text "it's friday!"
            text "it's not friday yet"
        ]
        route "/login" >=> redirectTo false loginUrl
        route "/oauth" >=> warbler (fun ctx -> 
            match (ctx.GetQueryStringValue "code", ctx.GetQueryStringValue "error") with
            | Ok c, _ -> 
                //finish oauth, store tokens, then redirect 
                getToken c
                |> AsyncResult.bind (storeToken storage)
                |> AsyncResult.map signIn
                |> handleAsyncResult id (fun e -> setStatusCode 500)
            | Error _, Ok err -> setStatusCode 400
            | _, _ -> setStatusCode 400
        )
        setStatusCode 404 >=> text "Not Found" 
    ] : HttpHandler