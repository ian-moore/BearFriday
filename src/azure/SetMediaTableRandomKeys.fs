module BearFriday.Azure.SetMediaTableRandomKeys

open BearFriday.Azure.Model
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Host
open Microsoft.WindowsAzure.Storage.Table
open System

let getRandomKeys (random: Random) =
    ( random.Next 8, random.Next 8, random.Next 8 )




let [<Literal>] FridaySchedule = "0 0 2 * * 5"

let [<Literal>] DebugSchedule = "0 0/1 * * * *"

[<FunctionName("SetMediaTableRandomKeys")>]
let run 
    ( [<TimerTrigger(DebugSchedule)>] timer: TimerInfo,
      [<Table("media", "config")>] config: CloudTable, 
      log: TraceWriter
    ) = 
    async {
        let (keyA, keyB, keyC) = Random() |> getRandomKeys
        let upsert = 
            ConfigEntity(
                PartitionKey = "config",
                RowKey = "config",
                RandomKeyA = keyA,
                RandomKeyB = keyB,
                RandomKeyC = keyC
            ) |> TableOperation.InsertOrReplace 
        let! result = config.ExecuteAsync upsert |> Async.AwaitTask
        return ()
    } |> Async.RunSynchronously