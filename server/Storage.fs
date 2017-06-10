module BearFriday.Storage

open FSharp.Azure.Storage.Table
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Table

type User = 
    { [<PartitionKey>] Object: string
      [<RowKey>] Id: string
      Username: string
      AccessToken: string }

let defaultUser =
  { Object = "user"
    Id = ""
    Username = ""
    AccessToken = "" }

type StorageClient (connectionString, tableName) =
    let a = CloudStorageAccount.Parse connectionString
    let tc = a.CreateCloudTableClient ()
    let inMyTable = inTable tc tableName
    let fromMyTable q = fromTable tc tableName q

    member this.InsertUser (x: User) =
        try
            InsertOrReplace x |> inMyTable |> Ok
        with
        | ex -> Error ex

    member this.FindUser id =
        try
            Query.all<User>
            |> Query.where <@ fun g s -> s.PartitionKey = "user" && s.RowKey = id @>
            |> fromMyTable
            |> Seq.head
            |> Ok
        with
        | ex -> Error ex