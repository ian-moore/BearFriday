module View exposing (render)

import Dict exposing (Dict)
import Html exposing (..)
import Html.Attributes exposing (..)
import Json.Encode as Encode
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


instagramImageHref : String -> String
instagramImageHref id = 
    "https://instagram.com/p/" ++ id

instagramImageSrc : String -> String
instagramImageSrc id =
    "https://instagram.com/p/" ++ id ++ "/media/?size=m"



instagramCard : InstagramEmbed -> Html Msg
instagramCard embed =
    div [ class "embed-root" ]
        [ div [ class "embed-author" ]
            [ a [ class "embed-author-link", href embed.authorurl ]
                [ text embed.authorname ]
            , span [] [ text " on Instagram" ]
            ]
        , a [ class "embed-photo-link"
            , href (instagramImageHref embed.id) 
            , target "_blank"
            ]
            [ div [ class "embed-photo" ]
                [ img [ src (instagramImageSrc embed.id) ]
                    []
                ]
            ]
        , div [ class "embed-description" ]
            [ span [] [ text embed.title ]
            ]
        , div [ class "embed-moretext" ]
            [ a [ class "embed-moretext-link"
                , href (instagramImageHref embed.id)
                , target "_blank" 
                ] 
                [ text "...more" ]
            ]
        ]


renderViewingMedia : App -> Html Msg
renderViewingMedia app =
    app.media
    |> List.filterMap (\m -> Dict.get m.externalId app.instagramEmbeds)
    |> List.map instagramCard
    |> div [ class "mediagrid-root" ]


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