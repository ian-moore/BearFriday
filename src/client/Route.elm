module Route exposing (parseRoute, Route(..))

import Navigation exposing (Location)
import UrlParser as Url


type Route
  = MainRoute
  | CurateRoute


router : Url.Parser (Route -> a) a
router =
  Url.oneOf
    [ Url.map MainRoute (Url.top)
    , Url.map CurateRoute (Url.s "curate")
    ]


parseRoute : Location -> Maybe Route
parseRoute = Url.parsePath router