module BearFriday.Server.Storage

open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Table

let immutable () = failwith "Property is immutable."

type User = 
    { Id: string
      Username: string
      AccessToken: string }

type private UserEntity(u: User) as this =
    inherit TableEntity()
    let e = (this :> TableEntity)
    do
        e.PartitionKey <- "user"
        e.RowKey <- u.Id
    member val Username = u.Username with get, set
    member val AccessToken = u.AccessToken with get, set
        


type BearMedia = | Instagram

type Media =
    { Type: BearMedia
      ExternalId: string
      AddedBy: string
      AddedOn: System.DateTime }

type StorageClient(connectionString: string, tableName: string) =
    let account = CloudStorageAccount.Parse connectionString
    let azureClient = account.CreateCloudTableClient ()
    let table = azureClient.GetTableReference tableName
    
    member __.StoreUser u = async {
            let op = UserEntity u |> TableOperation.InsertOrReplace
            let! r = table.ExecuteAsync op |> Async.AwaitTask
            match r.HttpStatusCode with
            | 200 -> return Ok (u.Id, u.Username)
            | _ -> return Error (exnf "StoreUser: InsertOrReplace failed with HTTP %i." r.HttpStatusCode)
        }

