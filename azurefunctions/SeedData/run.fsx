#if INTERACTIVE

open System

#I @"C:/Users/Ian/AppData/Roaming/npm/node_modules/azure-functions-core-tools/bin/"

#r "Microsoft.Azure.Webjobs.Host.dll"
open Microsoft.Azure.WebJobs.Host

#r "System.Net.Http.dll"
#r "System.Net.Http.Formatting.dll"
#r "System.Web.Http.dll"
#r "Newtonsoft.Json.dll"

#else

#r "System.Net.Http"
#r "Newtonsoft.Json"

#endif

open System.IO
open System.Net
open System.Net.Http
open Newtonsoft.Json

let Run(req: HttpRequestMessage, log: TraceWriter) =
    async {
        File.ReadAllText("./data.csv") |> log.Info
        return req.CreateResponse(HttpStatusCode.NoContent)
    } |> Async.RunSynchronously
