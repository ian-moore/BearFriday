#r "packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.YarnHelper
open System

let buildDir = "./build"

Target "Clean" (fun _ ->
    CleanDir buildDir
)

Target "InstallPackages" (fun _ ->
    DotNetCli.Restore (fun p -> 
        { p with
            WorkingDir = "./server" }
    )

    Yarn (fun p ->
        { p with
            WorkingDirectory = "./client"
            Command = Install Standard }
    )
)

Target "BuildClient" (fun _ ->
    Yarn (fun p ->
        { p with 
            WorkingDirectory = "./client"
            Command = Custom "build" }
    )

    CopyDir (buildDir </> "public") "./client/dist" allFiles
)

Target "BuildServer" (fun _ ->
    DotNetCli.Build (fun p ->
        { p with
            WorkingDir = "./server" }
    )

    CopyDir buildDir "./server/bin/Debug/net462" allFiles
)

Target "Default" DoNothing

"Clean"
    ==> "InstallPackages"
    ==> "BuildServer"
    ==> "BuildClient"
    ==> "Default"

RunTargetOrDefault "Default"