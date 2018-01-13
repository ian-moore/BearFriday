module BearFriday.Azure.ExtractSeedDataToQueue

open BearFriday.Azure.Model
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Host
open System.IO
open System.Net
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
    let m = Regex.Match(url, "https?://[w]{0,3}[.]?instagram.com/p/([\w-_]+)/")
    match m.Success with
    | true -> Some m.Groups.[1].Value
    | false -> None


[<FunctionName("ExtractSeedDataToQueue")>]
let run 
    ( [<HttpTrigger>] req: HttpRequestMessage,
      [<Blob("seed-data/seed_data_20180113.csv", FileAccess.Read)>] blob: Stream,
      [<Queue("media-queue")>] mediaQueue: IAsyncCollector<Media>,
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
        
        let msg = sprintf "Queued %i items." queueInserts.Length
        log.Info msg
        
        let resp = req.CreateResponse HttpStatusCode.OK
        resp.Content <- new StringContent(msg)

        return resp
    } |> Async.RunSynchronously