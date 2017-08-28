module BearFriday.Server.Storage

open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Table

type User = 
    { Id: string
      Username: string
      AccessToken: string }

type UserEntity(u: User) as this =
    inherit TableEntity()
    let e = (this :> TableEntity)
    do
        e.PartitionKey <- "user"
        e.RowKey <- u.Id
    member val Username = u.Username with get, set
    member val AccessToken = u.AccessToken with get, set


type MediaType = 
    | Instagram

type Media =
    { Type: MediaType
      ExternalId: string
      AddedBy: string
      AddedOn: System.DateTimeOffset }

type MediaEntity(m: Media) as this =
    inherit TableEntity()
    let e = (this :> TableEntity)
    do
        e.PartitionKey <- "media"
        e.RowKey <- sprintf "IG-%s" m.ExternalId

    new() = MediaEntity(
        { Type = Instagram
          ExternalId = ""
          AddedBy = ""
          AddedOn = System.DateTimeOffset.MinValue })

    member val ExternalId = m.ExternalId with get, set
    member val AddedBy = m.AddedBy with get, set
    member val AddedOn = m.AddedOn with get, set
    member val Type = m.Type with get, set

type StorageClient(connectionString: string, tableName: string) =
    let account = CloudStorageAccount.Parse connectionString
    let azureClient = account.CreateCloudTableClient ()
    let table = azureClient.GetTableReference tableName
    let createTable () = table.CreateIfNotExistsAsync () |> Async.AwaitTask
    let executeOperation = table.ExecuteAsync >> Async.AwaitTask
    
    let (|SuccessCode|_|) v = if v >= 200 && v < 300 then Some v else None

    member __.RetrieveMedia query count = async {
            let c = System.Nullable<int> count
            let tq = TableQuery<MediaEntity>().Where(query).Take(c)
            let rec loop (t: TableContinuationToken) m = async {
                    let! result = table.ExecuteQuerySegmentedAsync(tq, t) |> Async.AwaitTask
                    let m' = List.ofSeq result.Results |> List.append m 
                    return! loop result.ContinuationToken m'
                }
            return! loop null List.empty<MediaEntity>
        }

    member __.StoreUser u = async {
            let! c = createTable ()
            let! r = UserEntity u |> TableOperation.InsertOrReplace |> executeOperation
            match r.HttpStatusCode with
            | SuccessCode v -> return Ok (u.Id, u.Username)
            | _ -> return Error <| exnf "StoreUser: InsertOrReplace failed with HTTP %i." r.HttpStatusCode
        }

    member __.StoreMedia m = async {
            let! c = createTable ()
            let! r = MediaEntity m |> TableOperation.InsertOrReplace |> executeOperation
            match r.HttpStatusCode with
            | SuccessCode v -> return Ok m.ExternalId
            | _ -> return Error <| exnf "StoreMedia: InsertOrReplace failed with HTTP %i." r.HttpStatusCode
        }

