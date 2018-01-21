module Model exposing (..)

import Navigation exposing (Location)


type Msg
    = UrlChange Location
    | NoOp


type AppState
    = Loading
    | Done


type alias BearMedia =
    { externalId: String 
    }


type alias App = 
    { state: AppState
    , media: List BearMedia
    }
