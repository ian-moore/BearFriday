module BearFriday.Instagram

open Chiron
open Hopac
open HttpFs.Client

let buildAuthUrl =
    sprintf "https://api.instagram.com/oauth/authorize/?client_id=%s&redirect_uri=%s&response_type=code"

// {"access_token": "xxxxx.yyyyy.9dbc8491ead140e3973e4fb5819bfe61", 
//  "user": {"id": "xxxxx", "username": "foobar", 
//           "profile_picture": "https://scontent.cdninstagram.com/a.jpg", 
//           "full_name": "Ian Moore", "bio": "", "website": ""}}

type User =
    { Id: string
      Username: string
      ProfilePicture: string
      FullName: string
      Bio: string
      Website: string }
    static member FromJson(_:User) = json {
            let! id = Json.read "id"
            let! un = Json.read "username"
            let! pp = Json.read "profile_picture"
            let! fn = Json.read "full_name"
            let! b = Json.read "bio"
            let! w = Json.read "website"
            return { Id = id; Username = un; ProfilePicture = pp; FullName = fn; Bio = b; Website = w }
        }

type Token = 
    { AccessToken: string
      User: User }
    static member FromJson(_:Token) = json {
            let! t = Json.read "access_token"
            let! u = Json.read "user"
            return { AccessToken = t; User = u }
        }


let internal deserializeToken s : Result<Token,exn> = 
    try
        Json.parse s |> Json.deserialize |> Ok
    with
    | ex -> 
        sprintf "Error handling token: %s. Payload: %s" ex.Message s
        |> exn
        |> Result.Error

let requestAccessToken clientId clientSecret redirectUri code =
    Request.createUrl Post "https://api.instagram.com/oauth/access_token"
    |> Request.body (BodyForm
        [
            NameValue ("client_id", clientId)
            NameValue ("client_secret", clientSecret)
            NameValue ("grant_type", "authorization_code")
            NameValue ("redirect_uri", redirectUri)
            NameValue ("code", code)
        ])
    |> Request.responseAsString
    |> Job.map deserializeToken
    |> Job.toAsync