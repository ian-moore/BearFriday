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