module Main exposing (main)

import Http
import Json.Decode as Decode
import Model exposing (..)
import Navigation exposing (Location)
import Route exposing (parseRoute, Route(..))
import View


decodeMedia : Decode.Decoder BearMedia
decodeMedia =
    Decode.map3 BearMedia 
        (Decode.field "source" Decode.string)
        (Decode.field "externalId" Decode.string)
        (Decode.field "addedBy" Decode.int)


decodeMediaList : Decode.Decoder (List BearMedia)
decodeMediaList = Decode.list decodeMedia


loadMedia : () -> Cmd Msg
loadMedia () =
    let
        url = "http://bearfridayfunctions.azurewebsites.net/api/QueryCurrentMediaFromTable?code=f8Ksmt61UXXunK7AnUTSykMiFQamzt8VZbUS6CinrdAZ6WZkXYj1uA=="
    in
        Http.send MediaLoaded (Http.get url decodeMediaList)


initialApp : App
initialApp =
    { state = Loading
    , media = []
    }


init : Location -> (App, Cmd Msg)
init location =
    case parseRoute location of
        Just MainRoute ->
            initialApp ! [ loadMedia () ]
        Just CurateRoute ->
            initialApp ! []
        _ ->
            initialApp ! [ Navigation.modifyUrl "" ]


update : Msg -> App -> (App, Cmd Msg)
update msg app =
    case msg of
        NoOp ->
            app ! []
        MediaLoaded result ->
            case result of
                Ok mediaList ->
                    app ! []
                Err error ->
                    app ! []
        UrlChange location ->
            app ! []


main : Program Never App Msg
main =
    Navigation.program UrlChange
        { init = init
        , update = update
        , subscriptions = (\_ -> Sub.none)
        , view = View.render
        }
