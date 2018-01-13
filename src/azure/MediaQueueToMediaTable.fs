module BearFriday.Azure.SeedQueueToMediaTable

open BearFriday.Azure.Model
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Host
open System


[<FunctionName("MediaQueueToMediaTable")>]
let run 
    ( [<QueueTrigger("media-queue")>] media: Media,
      log: TraceWriter 
    ) : [<Table("media")>] MediaEntity = 
    MediaEntity(
        PartitionKey = media.Source,
        RowKey = media.ExternalId,
        AddedBy = media.AddedBy,
        AddedOn = DateTimeOffset.UtcNow
    )
        
        
