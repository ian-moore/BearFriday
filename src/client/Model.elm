module Model exposing (..)


type AppState
    = LoadingMedia
    | MediaError String
    | ViewingMedia


type alias BearMedia =
    { source: String
    , externalId: String 
    , addedBy: String
    }


type alias App = 
    { state: AppState
    , media: List BearMedia
    }