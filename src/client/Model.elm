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


type alias MediaEmbed =
    { id: String
    , html: String
    }


type alias App = 
    { state: AppState
    , media: List BearMedia
    , instagramEmbeds: Dict String MediaEmbed
    }


initialApp : App
initialApp =
    { state = LoadingMedia
    , media = []
    , instagramEmbeds = Dict.empty
    }