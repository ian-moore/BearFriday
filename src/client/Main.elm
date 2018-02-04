module Main exposing (main)

import Command exposing (loadMedia)
import Message exposing (..)
import Model exposing (..)
import Navigation exposing (Location)
import Route exposing (parseRoute, Route(..))
import Update
import View


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
            Update.mediaLoaded app result
        EmbedsLoaded result ->
            Update.embedsLoaded app result
        UrlChange location ->
            Debug.log "UrlChange"
            app ! []


main : Program Never App Msg
main =
    Navigation.program UrlChange
        { init = init
        , update = update
        , subscriptions = (\_ -> Sub.none)
        , view = View.render
        }
