module Main exposing (main)

import Model exposing (..)
import Navigation exposing (Location)
import Route exposing (parseRoute, Route(..))
import View


initialApp : App
initialApp =
    { state = Loading
    , media = []
    }


init : Location -> (App, Cmd Msg)
init location =
    case parseRoute location of
        Just MainRoute ->
            initialApp ! []
        Just CurateRoute ->
            initialApp ! []
        _ ->
            initialApp ! [ Navigation.modifyUrl "" ]


update : Msg -> App -> (App, Cmd Msg)
update msg app =
    app ! []


main : Program Never App Msg
main =
    Navigation.program UrlChange
        { init = init
        , update = update
        , subscriptions = (\_ -> Sub.none)
        , view = View.render
        }
