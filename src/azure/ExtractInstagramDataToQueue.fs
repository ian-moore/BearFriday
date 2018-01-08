module BearFriday.Azure.ExtractInstagramDataToQueue

open BearFriday.Azure.Model
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Host
open System.IO
open System.Net.Http
open System.Text
open System.Text.RegularExpressions


let readStream (s: Stream) =
    seq {
        use sr = new StreamReader(s, Encoding.UTF8)
        while (not sr.EndOfStream) do
            yield sr.ReadLine()
    }


let getIdFromShareUrl url =
    let m = Regex.Match(url, "https?://instagram.com/p/([\w-_]+)/")
    match m.Success with
    | true -> Some m.Groups.[1].Value
    | false -> None


[<FunctionName("ExtractInstagramDataToQueue")>]
let run 
    ( [<HttpTrigger>] req: HttpRequestMessage,
      [<Blob("seed-data/data.csv", FileAccess.Read)>] blob: Stream,
      [<Queue("seed-queue")>] mediaQueue: IAsyncCollector<Media>,
      log: TraceWriter
    ) = 
    async {
        let! queueInserts =
            readStream blob
            |> Seq.choose getIdFromShareUrl
            |> Seq.map (fun id -> 
                { Source = "instagram"; ExternalId = id; AddedBy = "4057131565" }
                |> mediaQueue.AddAsync
                |> Async.AwaitTask
            ) |> Async.Parallel
        
        log.Info <| sprintf "Queued %i items." queueInserts.Length
        return ()      
    } |> Async.RunSynchronously