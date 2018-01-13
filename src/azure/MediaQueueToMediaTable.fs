module BearFriday.Azure.MediaQueueToMediaTable

open BearFriday.Azure.Model
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Host
open System


let getRandomKeys (random: Random) =
    ( random.Next 8, random.Next 8, random.Next 8 )


[<FunctionName("MediaQueueToMediaTable")>]
let run 
    ( [<QueueTrigger("media-queue")>] media: Media,
      log: TraceWriter 
    ) : [<Table("media")>] MediaEntity = 
    let (keyA, keyB, keyC) = Random () |> getRandomKeys

    MediaEntity(
        PartitionKey = media.Source,
        RowKey = media.ExternalId,
        AddedBy = media.AddedBy,
        AddedOn = DateTimeOffset.UtcNow,
        RandomKeyA = keyA,
        RandomKeyB = keyB,
        RandomKeyC = keyC
    )
        
        
