module BearFriday.App

open BearFriday.Storage
open Suave
open Suave.Cookie
open Suave.Filters
open Suave.Logging
open Suave.Logging.Message
open Suave.Operators
open Suave.State.CookieStateStore
open System

let logger = Log.create "BearFriday.App"

type AppConfiguration = 
    { EnableFriday: bool
      EnableTuesday: bool
      InstagramClientId: string
      InstagramClientSecret: string
      InstagramRedirectUri: string 
      AzureConnection: string
      AzureTable: string }

let getCurrentDayOfWeek () = 
    let d = TimeSpan (-5, 0, 0) |> DateTimeOffset.UtcNow.ToOffset
    d.DayOfWeek

let dayIsFriday getDay ctx = async {
        match getDay () with
        | DayOfWeek.Friday -> return Some ctx
        | _ -> return None
    }
    
let dayIsTuesday getDay ctx = async {
        match getDay () with
        | DayOfWeek.Tuesday -> return Some ctx
        | _ -> return None
    }

let todayIsTuesday : WebPart = dayIsTuesday getCurrentDayOfWeek

let todayIsFriday : WebPart = dayIsFriday getCurrentDayOfWeek

let enableFeature toggle ctx = async {
        match toggle with
        | true -> return Some ctx
        | false -> return None
    }

let storeUserId id =
    context (fun ctx ->
        match HttpContext.state ctx with
        | None -> ServerErrors.INTERNAL_ERROR "Invalid cookie. Refresh the page to reset."
        | Some s -> s.set "instagram-id" id
    )

let completeOAuthRequest config =
    Instagram.requestAccessToken config.InstagramClientId config.InstagramClientSecret config.InstagramRedirectUri

let storeTokensInAzure (storage: StorageClient) (result: Async<Result<Instagram.Token,exn>>) = async {
        let! r = result
        match r with
        | Ok t ->
            return storage.InsertUser
                { defaultUser with
                    Id = t.User.Id 
                    Username = t.User.Username
                    AccessToken = t.AccessToken }
            |> Result.bind (fun o -> Ok t.User.Id)
        | Result.Error ex -> 
            logger.error (eventX "Instagram token error." >> addExn ex)
            return Result.Error ex
    }

let handleAuthResult (r: Async<Result<string,exn>>) = 
    fun ctx -> async {
        let! result = r
        match result with
        | Ok id -> 
            return! storeUserId id ctx 
            >>= (Authentication.authenticated Session false >=> Redirection.redirect "/curate")
        | Result.Error ex -> 
            logger.error (eventX "User id error." >> addExn ex)
            return! ServerErrors.INTERNAL_ERROR ex.Message ctx
    }

let verifyAuth protectedPart = 
    Authentication.authenticate CookieLife.Session false
        (fun () -> Redirection.redirect "/login" |> Choice2Of2)
        (fun _ ->  Redirection.redirect "/login" |> Choice2Of2)
        protectedPart


let createApp config = 
    let showBearFriday = enableFeature config.EnableFriday >=> todayIsFriday
    let showSquirrelTuesday = enableFeature config.EnableTuesday >=> todayIsTuesday
    let loginUrl = Instagram.buildAuthUrl config.InstagramClientId config.InstagramRedirectUri
    let storage = StorageClient (config.AzureConnection, config.AzureTable)
    // todo: refactor to use Result.bind
    let completeOAuth = 
        completeOAuthRequest config // todo: verify allowed users
        >> storeTokensInAzure storage
        >> handleAuthResult

    statefulForSession >=> choose [
        path "/" >=> choose [
            showBearFriday >=> context (fun c -> Successful.OK "it's friday!")
            showSquirrelTuesday >=> context (fun c -> Successful.OK "it's tuesday!")
            Files.file "splash.html"
        ]
        path "/login" >=> Redirection.redirect loginUrl
        path "/oauth" >=> context (fun ctx ->
            let req = ctx.request
            match (req.queryParam "code", req.queryParam "error") with
            | (Choice1Of2 code, _) -> completeOAuth code
            | (_, Choice1Of2 x) -> RequestErrors.BAD_REQUEST x
            | _ -> RequestErrors.BAD_REQUEST "Invalid parameters."
        )
        showBearFriday >=> choose [
            path "/api/bears" >=> Successful.OK "bear api call"
            path "/api/curate" >=> Successful.OK "bear api call"
        ]
        pathRegex "(.*)\.(css|png|gif|js|map|ico)" >=> Files.browseHome
        verifyAuth <| choose [
            path "/curate" >=> Successful.OK "curator page"
        ]
        RequestErrors.NOT_FOUND "Not found"
    ]
