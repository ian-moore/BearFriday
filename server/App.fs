module BearFriday.Server.App

open BearFriday.Server.Storage
open Giraffe
open Giraffe.HttpHandlers
open Giraffe.Razor.HttpHandlers
open Giraffe.Tasks
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open System
open System.Security.Claims
open System.Threading.Tasks

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

let dayIsFriday getDay = 
    fun next ctx -> task {
        match getDay () with
        | DayOfWeek.Friday -> return! next ctx
        | _ -> return None
    }
    
let todayIsFriday = dayIsFriday currentDayOfWeek

let enableFeature toggle =
    fun next ctx -> task {
        match toggle with
        | true -> return! next ctx
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
    fun (next: HttpFunc) (ctx: HttpContext) -> task {
        let issuer = "http://localhost:5000"
        let claims = 
            [ Claim(ClaimTypes.Name, username, ClaimValueTypes.String, issuer)
              Claim(ClaimTypes.NameIdentifier, userId, ClaimValueTypes.String, issuer) ]
        let user = ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme) |> ClaimsPrincipal
        do! ctx.SignInAsync user
        return! next ctx
    }

let getUserFromClaims (ctx: HttpContext) =
    let userId = ctx.User.FindFirst ClaimTypes.NameIdentifier
    let username = ctx.User.FindFirst ClaimTypes.Name
    userId.Value, username.Value

let handleAsyncResult (okFunc: 'a -> HttpHandler) (errorFunc: 'b -> HttpHandler) ar =
    fun next ctx -> task {
        let! r = ar |> Async.StartAsTask
        match r with
        | Ok v -> 
            return! okFunc v next ctx
        | Error ex -> 
            return! errorFunc ex next ctx
    }

[<CLIMutable>]
type NewMedia =
    { IsFormSubmission: bool
      InstagramUrl: string }

let addBearMedia (storage: StorageClient) =
    let igError = setStatusCode 400 >=> text "Invalid Instagram URL."
    fun next (ctx: HttpContext) -> task {
        let! media = ctx.BindFormAsync<NewMedia> ()
        let parsedId = Instagram.getIdFromShareUrl media.InstagramUrl
        let user = getUserFromClaims ctx
        match parsedId with
        | Some id -> 
            return!
                { Type = Instagram
                  ExternalId = id
                  AddedBy = (fst user)
                  AddedOn = System.DateTimeOffset.UtcNow }
                |> storage.StoreMedia
                |> handleAsyncResult
                    (fun v -> setStatusCode 204)
                    (fun ex -> setStatusCode 500 >=> text ex.Message)
                |> (fun f -> f next ctx)
        | None -> 
            return! igError next ctx
    }

let getBearMedia (storage: StorageClient) =
    let mediaCount = 100
    fun next (ctx: HttpContext) -> task {
        let! x = storage.RetrieveMedia "" mediaCount
        return! next ctx
    }

let errorResponse (ex: exn) next ctx =
    (clearResponse >=> setStatusCode 500 >=> text ex.Message) next ctx

let errorHandler (ex: exn) (logger: ILogger) ctx =
    logger.LogError(EventId(0), ex, "An unhandled exception has occurred while executing the request.")
    errorResponse ex ctx

let createApp config : HttpHandler = 
    let showBearFriday = choose [ todayIsFriday ; enableFeature config.EnableFriday ] 
    let loginUrl = Instagram.buildAuthUrl config.InstagramClientId config.InstagramRedirectUri
    let getToken = requestInstagramToken config
    let storage = StorageClient (config.AzureConnectionString, config.AzureTableName)
    let requireLogin = requiresAuthentication (challenge authScheme)
    
    choose [
        route "/" >=> choose [
            showBearFriday >=> razorHtmlView "Friday" ()
            razorHtmlView "Splash" ()
        ]
        route "/login" >=> redirectTo false loginUrl
        route "/oauth" >=> warbler (fun (next, ctx) -> 
            match (ctx.GetQueryStringValue "code", ctx.GetQueryStringValue "error") with
            | Ok c, _ -> // finish oauth, store tokens, then redirect 
                getToken c 
                |> AsyncResult.bind (storeToken storage)
                |> AsyncResult.map signIn
                |> handleAsyncResult id errorResponse
                >=> redirectTo false "/curate"
            | Error _, Ok err -> setStatusCode 400
            | _, _ -> setStatusCode 400
        )
        subRoute "/api" 
            (choose [
                route "/bears" >=> (getBearMedia storage)
                route "/curate" >=> requireLogin >=> choose [
                    POST >=> (addBearMedia storage)
                    DELETE >=> text "delete a bear media."
                ]
            ])
        route "/curate" >=> requireLogin >=> razorHtmlView "Curate" ()
        route "/logout" 
            >=> requireLogin
            >=> signOff CookieAuthenticationDefaults.AuthenticationScheme 
            >=> redirectTo false "/"
        setStatusCode 404 >=> text "Not Found" 
    ]