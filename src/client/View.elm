module View exposing (render)

import Html exposing (..)
import Html.Attributes exposing (..)
import Message exposing (..)
import Model exposing (..)


loadingSpinner : () -> Html Msg
loadingSpinner () =
    div [ class "lds-grid" ]
        [ div [] []
        , div [] []
        , div [] []
        , div [] []
        , div [] []
        , div [] []
        , div [] []
        , div [] []
        , div [] []
        ]


renderLoadingMedia : App -> Html Msg
renderLoadingMedia app =
    div [ class "loading-root" ] 
        [ loadingSpinner ()
        ]


renderViewingMedia : App -> Html Msg
renderViewingMedia app =
    div [ class "mediagrid-root" ] 
        [ text "Viewing" ]


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
    div [ id "bear-friday" ]
        [ div [ class "header-root" ]
            [ h1 [ class "header-h1" ]
                [ text  "Bear Friday" ]
            , div [ class "header-note" ] 
                [ text "See new bears every Friday!" ] 
            ]
        , div [ class "container-root" ]
            [ renderBody app
            ]
        ]