module BearFriday.Azure.QueryCurrentMediaFromTable

open BearFriday.Azure.Model
open Microsoft.WindowsAzure.Storage.Table
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Host
open System
open System.Net.Http


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
                    "RandomKeyA", 
                    QueryComparisons.Equal, 
                    Convert.ToString config.RandomKeyA
                ),
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "RandomKeyB", 
                    QueryComparisons.Equal, 
                    Convert.ToString config.RandomKeyB
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
        match isNull segment.ContinuationToken with
        | true -> return media
        | false ->
            return!
                Seq.append media segment.Results
                |> getMedia table query segment.ContinuationToken
    }

        


[<FunctionName("QueryCurrentMediaFromTable")>]
let run 
    ( [<HttpTrigger>] req: HttpRequestMessage,
      [<Table("media", "media")>] mediaTable: CloudTable, 
      [<Table("media", "config")>] configTable: CloudTable, 
      log: TraceWriter
    ) = 
    async {
        let! config = getConfig configTable
        let query = getTableQuery config

        let! entities = getMedia mediaTable query null Seq.empty

        let sorted = 
            match config.RandomKeyC >= 4 with
            | true -> entities |> Seq.sortBy (fun m -> m.RandomKeyC)
            | false -> entities |> Seq.sortByDescending (fun m -> m.RandomKeyC)

        return
            Seq.take 20 sorted
            |> Seq.map recordFromMediaEntity
    } |> Async.RunSynchronously