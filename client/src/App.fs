module BearFriday.Client

open Fable.Core
open Fable.Core.JsInterop
open Fable.Import

module R = Fable.Helpers.React

type AppState = { Loading: bool }
type App(props, s) as this =
    inherit React.Component<obj,AppState>(props, s)
    member this.render () =
        R.div [] [unbox "Hello world!"]

let init _ =
    ReactDom.render (
        R.com<App,_,_> { Loading = true } [],
        Browser.document.getElementById "app"
    )


Browser.window.addEventListener ("DOMContentLoaded", unbox init)