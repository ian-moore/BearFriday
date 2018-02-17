module BearFriday.Azure.QueryCurrentMediaFromTable

open BearFriday.Azure.Model
open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Host
open Microsoft.WindowsAzure.Storage.Table
open System
open System.Net.Http
open Microsoft.Azure.WebJobs.Extensions.Http


let getConfig (table: CloudTable) =
    async {
        let! retrieve =
            TableOperation.Retrieve<ConfigEntity>("config", "config")
            |> table.ExecuteAsync
            |> Async.AwaitTask
        
        match isNull retrieve.Result with
        | false -> return retrieve.Result :?> ConfigEntity
        | true -> return raise (Exception "Could not retrieve config")
    }


let getTableQuery (config: ConfigEntity) =
    TableQuery<MediaEntity>()
        .Where(
            TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.Equal,
                    "instagram"
                ),
                TableOperators.And,
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterConditionForInt(
                        "RandomKeyA", 
                        QueryComparisons.Equal, 
                        config.RandomKeyA
                    ),
                    TableOperators.And,
                    TableQuery.GenerateFilterConditionForInt(
                        "RandomKeyB", 
                        QueryComparisons.Equal, 
                        config.RandomKeyB
                    )
                )
            )
        )


let rec getMedia 
    (table: CloudTable) 
    (query: TableQuery<MediaEntity>)
    (token: TableContinuationToken) 
    (media: MediaEntity seq) =
    async {
        let! segment = 
            table.ExecuteQuerySegmentedAsync(query, token) 
            |> Async.AwaitTask
        
        let results = Seq.append media segment.Results
        match isNull segment.ContinuationToken with
        | true -> return results
        | false ->
            return! getMedia table query segment.ContinuationToken results
    }


let flip f x y = f y x


[<FunctionName("QueryCurrentMediaFromTable")>]
let run 
    ( [<HttpTrigger(AuthorizationLevel.Anonymous, "get")>] req: HttpRequestMessage,
      [<Table("media")>] table: CloudTable, 
      log: TraceWriter
    ) = 
    async {
        let! config = getConfig table
        log.Info
            <| sprintf "Config random keys - A: %i B: %i C: %i" 
                config.RandomKeyA 
                config.RandomKeyB 
                config.RandomKeyC

        let query = getTableQuery config
        let! tableRows = getMedia table query null Seq.empty

        let sorted = 
            match config.RandomKeyC >= 4 with
            | true -> tableRows |> Seq.sortBy (fun m -> m.RandomKeyC)
            | false -> tableRows |> Seq.sortByDescending (fun m -> m.RandomKeyC)
        
        let rowCount = Seq.length sorted;
        log.Info <| sprintf "Retrieved %i rows" rowCount

        let media =
            rowCount < 20
            |> (function | true -> rowCount | false -> 20)
            |> flip Seq.take sorted
            |> Seq.map recordFromMediaEntity
        
        return ObjectResult media
    } |> Async.RunSynchronously