
# BearFriday

F# ASP.NET Core web app using [Giraffe](https://github.com/dustinmoris/Giraffe)

Dev requirements:

* .NET Core SDK
* Azure Storage Explorer 
* Azure Storage Emulator
* Azure Functions CLI

Needed environment values:

* ASPNETCORE_ENVIRONMENT: 'Development' or 'Production
* InstagramClientSecret: Instagram app client secret

To build and run locally:

* Create table in local Azure Storage
* `build` to build
* `build -run` to run the app

To deploy Azure functions:

* Run `func azure functionapp publish bearfridayfunction` from the `/azurefunctions` directory