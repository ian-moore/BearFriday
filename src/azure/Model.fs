module BearFriday.Azure.Model


open Microsoft.WindowsAzure.Storage.Table
open System


[<CLIMutable>]
type Media = {
    Source: string
    ExternalId: string
    AddedBy: string
}


type MediaEntity() = 
    inherit TableEntity()
    member val AddedBy = "" with get, set
    member val AddedOn = DateTimeOffset.UtcNow with get, set
    
    member val RandomKeyA = -1 with get, set
    member val RandomKeyB = -1 with get, set
    member val RandomKeyC = -1 with get, set


let recordFromMediaEntity (m: MediaEntity) =
    { Source = m.PartitionKey; ExternalId = m.RowKey; AddedBy = m.AddedBy }


type ConfigEntity() =
    inherit TableEntity()

    member val RandomKeyA = -1 with get, set
    member val RandomKeyB = -1 with get, set
    member val RandomKeyC = -1 with get, set
