namespace BearFriday.Server

open BearFriday.Server.App
open Giraffe.Middleware
open Giraffe.Razor.Middleware
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open System
open System.IO

type Startup(env: IHostingEnvironment) =

    let cookieOptions (o : CookieAuthenticationOptions) =
        o.Cookie.HttpOnly <- true
        o.Cookie.SecurePolicy <- Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest
        o.SlidingExpiration <- true
        o.ExpireTimeSpan <- TimeSpan.FromDays 30.0
        o.LoginPath <- PathString "/login"
        o.LogoutPath <- PathString "/logout"
    
    let authenticationOptions (o: AuthenticationOptions) =
        o.DefaultAuthenticateScheme <- CookieAuthenticationDefaults.AuthenticationScheme
        o.DefaultChallengeScheme <- CookieAuthenticationDefaults.AuthenticationScheme
    
    member __.Configure(app: IApplicationBuilder) (env: IHostingEnvironment) (logger: ILoggerFactory) =
        let configBuilder = 
            ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()

        let config = configBuilder.Build()
        let appSettings = 
            { EnableFriday = config.["EnableFriday"] |> Convert.ToBoolean; 
              AzureConnectionString = config.["AzureConnectionString"]; 
              AzureTableName = config.["AzureTableName"]
              InstagramClientId = config.["InstagramClientId"] 
              InstagramClientSecret = config.["InstagramClientSecret"]
              InstagramRedirectUri = config.["InstagramRedirectUri"] }

        logger.AddConsole(LogLevel.Error).AddDebug() |> ignore
        
        app.UseGiraffeErrorHandler errorHandler |> ignore
        app.UseAuthentication () |> ignore
        app.UseStaticFiles() |> ignore
        createApp appSettings |> app.UseGiraffe

    member __.ConfigureServices(services: IServiceCollection) =
        let sp = services.BuildServiceProvider()
        let env = sp.GetService<IHostingEnvironment>()
        let viewsFolderPath = Path.Combine(env.ContentRootPath, "Views")
        services.AddRazorEngine viewsFolderPath |> ignore
        
        let authBuilder = 
            services.AddAuthentication (Action<AuthenticationOptions> authenticationOptions)
        authBuilder.AddCookie (Action<CookieAuthenticationOptions> cookieOptions)
        