import Html exposing (..)

friday =
  Html.program
    { init = init
    , view = view
    , update = update
    , subscriptions = subscriptions
    }


-- Model
type alias Model = 
  { photoIds : List String
  }


init : (Model, Cmd Msg)
init =
  (Model [], Cmd.none)


-- Update
type Msg
  = None
  | AddPhotos (List String)
  | ResetPhotos


update : Msg -> Model -> (Model, Cmd Msg)
update msg model =
  case msg of
    None -> 
      (model, Cmd.none)

    AddPhotos newIds ->
        ({ model | photoIds = List.append model.photoIds newIds }, Cmd.none)
    
    ResetPhotos ->
      ({model | photoIds = []}, Cmd.none)


-- Subscriptions
subscriptions : Model -> Sub Msg
subscriptions model = 
  Sub.none


-- View
view : Model -> Html Msg
view model =
  div [] [text "Hello world!"]