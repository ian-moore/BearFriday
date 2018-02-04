module Update exposing (mediaLoaded, embedsLoaded)

import Command exposing (loadInstagramEmbeds)
import Dict exposing (Dict)
import Http
import Message exposing (..)
import Model exposing (..)


mediaLoaded : App -> Result Http.Error (List BearMedia) -> (App, Cmd Msg)
mediaLoaded app result =
    case result of
        Ok mediaList ->
            { app
            | media = mediaList
            } ! [ loadInstagramEmbeds mediaList ]
        Err error ->
            { app
            | state = MediaError "TODO: Http error loading media from azure"
            } ! []


dictFromEmbedsList : List MediaEmbed -> Dict String MediaEmbed
dictFromEmbedsList = List.map (\e -> (e.id, e)) >> Dict.fromList


embedsLoaded : App -> Result Http.Error (List MediaEmbed) -> (App, Cmd Msg)
embedsLoaded app result =
    case result of
        Ok embeds ->
            { app
            | state = ViewingMedia
            , instagramEmbeds = dictFromEmbedsList embeds
            } ! []
        Err error ->
            { app
            | state = MediaError "TODO: Http error loading instagram embeds"
            } ! []