namespace BearFriday.Server

open BearFriday.Server.App
open Giraffe.Middleware
open Giraffe.Razor.Middleware
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open System
open System.IO

type Startup(env: IHostingEnvironment) =

    member __.Configure(app: IApplicationBuilder) (env: IHostingEnvironment) (logger: ILoggerFactory) =
        let configBuilder = 
            ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()

        let config = configBuilder.Build()
        let appSettings = 
            { EnableFriday = config.["EnableFriday"] |> Convert.ToBoolean; 
              AzureConnection = config.["AzureConnectionString"]; 
              AzureTableName = config.["AzureTableName"] }

        logger.AddConsole(LogLevel.Error).AddDebug() |> ignore
        
        app.UseGiraffeErrorHandler errorHandler
        app.UseStaticFiles() |> ignore
        createApp appSettings |> app.UseGiraffe

    member __.ConfigureServices(services: IServiceCollection) =
        let sp = services.BuildServiceProvider()
        let env = sp.GetService<IHostingEnvironment>()
        let viewsFolderPath = Path.Combine(env.ContentRootPath, "Views")
        services.AddRazorEngine viewsFolderPath |> ignore