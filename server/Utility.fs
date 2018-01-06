[<AutoOpen>]
module BearFriday.Server.Utility

/// Create an exception with formatted string message.
let exnf f = sprintf f >> exn

type AsyncResult<'a,'b> = Async<Result<'a,'b>>

[<RequireQualifiedAccess>]
module AsyncResult =
    /// Asynchronous bind for Async<Result<'a,'b>>
    let bind (f: 'a -> AsyncResult<'c,'b>) (x: AsyncResult<'a,'b>) = async {
            printfn "%s" "AsyncResult.bind"
            let! r = x
            match r with
            | Ok v -> return! f v
            | Error e -> return Error e
        }

    /// Map an Ok value contained in an Async<Result<'a,'b>>
    let map (f: 'a -> 'b) (x: AsyncResult<'a,'c>) = async {
            let! r = x
            match r with
            | Ok v -> return f v |> Ok
            | Error e -> return Error e
        }