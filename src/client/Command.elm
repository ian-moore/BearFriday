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
        url = "//bearfridayfunctions.azurewebsites.net/api/QueryCurrentMediaFromTable?code=f8Ksmt61UXXunK7AnUTSykMiFQamzt8VZbUS6CinrdAZ6WZkXYj1uA=="
    in
        Http.send MediaLoaded (Http.get url decodeMediaList)


decodeEmbedResponse : String -> Decode.Decoder InstagramEmbed
decodeEmbedResponse id = 
    Decode.map6 InstagramEmbed
        (Decode.succeed id)
        (Decode.field  "html" Decode.string)
        (Decode.field  "author_name" Decode.string)
        (Decode.field  "author_url" Decode.string)
        (Decode.field  "title" Decode.string)
        (Decode.field  "type" Decode.string)


getEmbedRequestForId : String -> Http.Request InstagramEmbed
getEmbedRequestForId id =
    let
        url = "https://api.instagram.com/oembed?url=http://instagr.am/p/" ++ id ++ "/&omitscript=true"
    in
        Http.get url (decodeEmbedResponse id)


loadInstagramEmbeds : List BearMedia -> Cmd Msg
loadInstagramEmbeds =
    List.map (\m -> m.externalId)
    >> List.map (getEmbedRequestForId >> Http.toTask)
    >> Task.sequence
    >> Task.attempt EmbedsLoaded