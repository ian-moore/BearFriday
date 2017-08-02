module BearFriday.Server.Instagram

open Newtonsoft.Json
open System.Net.Http
open System.Text.RegularExpressions

type InstagramUser =
    { Id: string
      Username: string
      ProfilePicture: string
      FullName: string
      Bio: string
      Website: string }

type Token = 
    { AccessToken: string
      User: InstagramUser }
    
let buildAuthUrl =
    sprintf "https://api.instagram.com/oauth/authorize/?client_id=%s&redirect_uri=%s&response_type=code"

let getIdFromShareUrl url =
    let m = Regex.Match(url, "https?://instagram.com/p/([\w]+)/")
    match m.Success with
    | true -> Some m.Groups.[1].Value
    | false -> None

let private httpClient = new HttpClient()

let deserializeToken s = 
    try
        s |> JsonConvert.DeserializeObject<Token> |> Ok
    with
    | ex ->
        sprintf "Error handling token: %s. Payload: %s" ex.Message s
        |> exn
        |> Result.Error

let requestAccessToken clientId clientSecret redirectUri code = async {
        let requestParams =
            dict [
                ("client_id", clientId)
                ("client_secret", clientSecret)
                ("grant_type", "authorization_code")
                ("redirect_uri", redirectUri)
                ("code", code)
            ]
        
        let postData = new FormUrlEncodedContent(requestParams)
        let! resp = 
            httpClient.PostAsync("https://api.instagram.com/oauth/access_token", postData)
            |> Async.AwaitTask
        
        let! respString = resp.Content.ReadAsStringAsync() |> Async.AwaitTask
        return deserializeToken respString
    }