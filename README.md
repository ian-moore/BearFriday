# BearFriday

See new bears every Friday!

Elm frontend with F# Azure Functions backend.

## Develop
* Install .NET Core 2.0
* Install Azure Functions CLI
```
npm install -g azure-functions-core-tools@core
```
* Install Invoke-Build
```
PS> Install-Module InvokeBuild
```
* Add `src/azure/local.settings.json` file:
```
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true;"
  }
}
```

## Build
```
PS> invoke-build
```

## Test
```
PS> invoke-build test
```

## Deploy

Deploy azure functions from the publish folder
```
PS> func azure login
PS> func azure functionapp publish BearFridayFunctions
```
