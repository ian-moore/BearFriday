module View exposing (render)

import Html exposing (..)
import Model exposing (..)

render : App -> Html Msg
render app =
    div [] [ text  "BearFriday" ]