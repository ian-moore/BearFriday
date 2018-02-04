module Message exposing (..)

import Http
import Model exposing (..)
import Navigation exposing (Location)

type Msg
    = UrlChange Location
    | NoOp
    | EmbedsLoaded (Result Http.Error (List MediaEmbed))
    | MediaLoaded (Result Http.Error (List BearMedia))