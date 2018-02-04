module Command exposing (loadInstagramEmbeds, loadMedia)

import Http
import Json.Decode as Decode
import Message exposing (..)
import Model exposing (..)
import Task


decodeMedia : Decode.Decoder BearMedia
decodeMedia =
    Decode.map3 BearMedia 
        (Decode.field "source" Decode.string)
        (Decode.field "externalId" Decode.string)
        (Decode.field "addedBy" Decode.string)


decodeMediaList : Decode.Decoder (List BearMedia)
decodeMediaList = Decode.list decodeMedia


loadMedia : () -> Cmd Msg
loadMedia () =
    let
        url = "http://bearfridayfunctions.azurewebsites.net/api/QueryCurrentMediaFromTable?code=f8Ksmt61UXXunK7AnUTSykMiFQamzt8VZbUS6CinrdAZ6WZkXYj1uA=="
    in
        Http.send MediaLoaded (Http.get url decodeMediaList)


decodeEmbedResponse : String -> Decode.Decoder MediaEmbed
decodeEmbedResponse id = Decode.map2 MediaEmbed
    (Decode.succeed id)
    (Decode.at [ "html" ] Decode.string)


getEmbedRequestForId : String -> Http.Request MediaEmbed
getEmbedRequestForId id =
    let
        url = "https://api.instagram.com/oembed?url=http://instagr.am/p/" ++ id ++ "/"
    in
        Http.get url (decodeEmbedResponse id)


loadInstagramEmbeds : List BearMedia -> Cmd Msg
loadInstagramEmbeds =
    List.map (\m -> m.externalId)
    >> List.map (getEmbedRequestForId >> Http.toTask)
    >> Task.sequence
    >> Task.attempt EmbedsLoaded