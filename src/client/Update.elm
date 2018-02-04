module Update exposing (..)


import Http
import Message exposing (..)
import Model exposing (..)


mediaLoaded : App -> Result Http.Error (List BearMedia) -> (App, Cmd Msg)
mediaLoaded app result =
    case result of
        Ok mediaList ->
            { app
            | state = ViewingMedia
            , media = mediaList
            } ! []
        Err error ->
            { app
            | state = MediaError "TODO: Http error"
            } ! []