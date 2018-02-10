module Model exposing (..)

import Dict exposing (Dict)


type AppState
    = LoadingMedia
    | MediaError String
    | ViewingMedia


type alias BearMedia =
    { source: String
    , externalId: String 
    , addedBy: String
    }


type alias InstagramEmbed =
    { id: String
    , html: String
    , authorname: String
    , authorurl: String
    , title: String
    , type_: String
    }


type alias App = 
    { state: AppState
    , media: List BearMedia
    , instagramEmbeds: Dict String InstagramEmbed
    }


initialApp : App
initialApp =
    { state = LoadingMedia
    , media = []
    , instagramEmbeds = Dict.empty
    }