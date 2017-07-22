module BearFriday.Server.App

open System
open System.IO
open System.Collections.Generic
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Server.IISIntegration
open BearFriday.Server

[<EntryPoint>]
let main argv =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot = Path.Combine(contentRoot, "WebRoot")

    WebHostBuilder()
        .UseKestrel()
        .UseIISIntegration()
        .UseContentRoot(contentRoot)
        .UseWebRoot(webRoot)
        .UseStartup<Startup>()
        .Build()
        .Run()
    0