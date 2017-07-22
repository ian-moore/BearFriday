namespace BearFriday.Server

open BearFriday.Server.Handlers
open Giraffe.Middleware
open Giraffe.Razor.Middleware
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Configuration.UserSecrets
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open System
open System.IO

type Startup(env: IHostingEnvironment) =

    member __.Configure(app: IApplicationBuilder) (env: IHostingEnvironment) (logger: ILoggerFactory) =
        let configBuilder = 
            ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json")
                .AddEnvironmentVariables()

        if env.IsDevelopment()
        then configBuilder.AddUserSecrets<Startup>() |> ignore

        let config = configBuilder.Build()

        logger.AddConsole(LogLevel.Error).AddDebug() |> ignore
        
        app.UseGiraffeErrorHandler errorHandler
        app.UseStaticFiles() |> ignore
        app.UseGiraffe webApp

    member __.ConfigureServices(services: IServiceCollection) =
        let sp = services.BuildServiceProvider()
        let env = sp.GetService<IHostingEnvironment>()
        let viewsFolderPath = Path.Combine(env.ContentRootPath, "Views")
        services.AddRazorEngine viewsFolderPath |> ignore