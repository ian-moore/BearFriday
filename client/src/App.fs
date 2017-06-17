module BearFriday.Client

open Fable.Core
open Fable.Core.JsInterop
open Fable.Import

module R = Fable.Helpers.React

type InstagramPhotoProps = { ShortCode: string }

type InstagramPhoto(props) as this =
    inherit React.Component<InstagramPhotoProps,obj>(props)

    member this.render () =
        R.div [] [unbox "IG Photo"]

type AppState = { Loading: bool }

type App(props) as this =
    inherit React.Component<obj,AppState>(props)
    member this.render () =
        R.div [] [unbox "Hello world!"]

let init _ =
    ReactDom.render (
        R.com<App,_,_> { Loading = true } [],
        Browser.document.getElementById "app"
    )

Browser.window.addEventListener ("DOMContentLoaded", unbox init)