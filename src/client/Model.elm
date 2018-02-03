module Model exposing (..)

import Http
import Navigation exposing (Location)


type AppState
    = Loading
    | Done


type alias BearMedia =
    { source: String
    , externalId: String 
    , addedBy: Int
    }


type alias App = 
    { state: AppState
    , media: List BearMedia
    }


type Msg
    = UrlChange Location
    | NoOp
    | MediaLoaded (Result Http.Error (List BearMedia))
