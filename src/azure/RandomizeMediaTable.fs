module BearFriday.Azure.RandomizeMediaTable

open BearFriday.Azure.Model
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Host
open Microsoft.WindowsAzure.Storage.Table
open System.Linq

let run (media: IQueryable<MediaEntity>, media2: CloudTable, log: TraceWriter) =
    // https://stackoverflow.com/questions/36792547/azure-functions-table-binding-how-do-i-update-a-row
    let mediaCount = media.Count ()
    let values = Array.create mediaCount 0
    let media2Client = media2.ServiceClient
    ()