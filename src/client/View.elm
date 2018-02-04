module View exposing (render)

import Html exposing (..)
import Message exposing (..)
import Model exposing (..)


renderLoadingMedia : App -> Html Msg
renderLoadingMedia app =
    div [] [ text "Loading" ]


renderViewingMedia : App -> Html Msg
renderViewingMedia app =
    div [] [ text "Viewing" ]


renderBody : App -> Html Msg
renderBody app =
    case app.state of
        LoadingMedia ->
            renderLoadingMedia app
        ViewingMedia ->
            renderViewingMedia app
        _ ->
            div [] [ text "todo" ]


render : App -> Html Msg
render app =
    div []
        [ text  "BearFriday"
        , renderBody app
        ]