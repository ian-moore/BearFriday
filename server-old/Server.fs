open BearFriday.App
open Microsoft.Azure
open Suave
open Suave.Logging
open Suave.Operators
open System
open System.IO
open System.Net

let appConfig = 
    { EnableFriday = CloudConfigurationManager.GetSetting "EnableFriday" |> Convert.ToBoolean
      EnableTuesday = false
      InstagramClientId = CloudConfigurationManager.GetSetting "IGClientId"
      InstagramClientSecret = CloudConfigurationManager.GetSetting "IGClientSecret"
      InstagramRedirectUri = CloudConfigurationManager.GetSetting "IGRedirectUri"
      AzureConnection = CloudConfigurationManager.GetSetting "AzureStorageConnectionString"
      AzureTable = CloudConfigurationManager.GetSetting "AzureStorageTableName" }

[<EntryPoint>]
let main argv =
    let port = Array.tryHead argv |> Option.defaultValue "8080" |> Sockets.Port.Parse
    let suaveConfig = 
        { defaultConfig with 
            bindings = [HttpBinding.create HTTP IPAddress.Loopback port]
            homeFolder = Some (Path.GetFullPath ".") }
    
    printfn "%s\n%A\n%s" "App Configuration:" appConfig "Starting Suave..."

    let logger = Targets.create Info [| "Suave"; "BearFriday" |]
    let app = createApp appConfig >=> Filters.logWithLevelStructured Info logger Filters.logFormatStructured
    app |> startWebServer suaveConfig
    0
