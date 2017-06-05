open BearFriday.App
open Microsoft.Azure
open Suave
open System
open System.Net

let appConfig = 
    { EnableFriday = CloudConfigurationManager.GetSetting "EnableFriday" |> Convert.ToBoolean
      EnableTuesday = false
      InstagramClientId = CloudConfigurationManager.GetSetting "IGClientId"
      InstagramClientSecret = CloudConfigurationManager.GetSetting "IGClientSecret"
      InstagramRedirectUri = CloudConfigurationManager.GetSetting "IGRedirectUri" }

[<EntryPoint>]
let main argv =
    let port = Array.tryHead argv |> Option.defaultValue "8080" |> Sockets.Port.Parse
    let suaveConfig = 
        { defaultConfig with 
            bindings = [HttpBinding.create HTTP IPAddress.Loopback port] }
    
    printfn "%s\n%A\n%s" "App Configuration:" appConfig "Starting Suave..."

    createApp appConfig |> startWebServer suaveConfig
    0
