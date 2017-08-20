#if INTERACTIVE

open System

#I @"C:/Users/Ian/AppData/Roaming/npm/node_modules/azure-functions-core-tools/bin/"

#r "Microsoft.Azure.WebJobs.dll"
#r "Microsoft.Azure.Webjobs.Host.dll"

open Microsoft.Azure.WebJobs
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
open System.Text
open System.Text.RegularExpressions
open System.Threading.Tasks

[<CLIMutable>]
type Media = 
    { PartitionKey: string
      RowKey: string
      ExternalId: string
      AddedBy: string
      AddedOn: DateTimeOffset }

let inline awaitPlainTask (task: Task) = 
    let continuation (t : Task) =
        match t.IsFaulted with
        | true -> raise t.Exception
        | arg -> ()
    task.ContinueWith continuation |> Async.AwaitTask

let storeMediaInTable (table: IAsyncCollector<Media>) (m: Media) = 
    table.AddAsync m |> awaitPlainTask

/// parse instagram id from the share url
let getIdFromShareUrl url =
    let m = Regex.Match(url, "https?://instagram.com/p/([\w-_]+)/")
    match m.Success with
    | true -> Some m.Groups.[1].Value
    | false -> None


/// read file stream into string per line
let readStream (s: Stream) =
    seq {
        use sr = new StreamReader(s, Encoding.UTF8)
        while (not sr.EndOfStream) do
            yield sr.ReadLine()
    }

/// main function method
let Run(req: HttpRequestMessage, inputBlob: Stream, outputTable: IAsyncCollector<Media>, log: TraceWriter) =
    let storeMedia = storeMediaInTable outputTable
    async {
        let! inserts = 
            readStream inputBlob
            |> Seq.choose getIdFromShareUrl
            |> Seq.map (fun id -> 
                { PartitionKey = "media"
                  RowKey = sprintf "IG-%s" id 
                  ExternalId = id
                  AddedBy = "4057131565"
                  AddedOn = DateTimeOffset.UtcNow }
                |> storeMedia)
            |> Async.Parallel
        
        return req.CreateResponse(HttpStatusCode.OK, sprintf "Inserted %i objects." inserts.Length)
    } |> Async.RunSynchronously
