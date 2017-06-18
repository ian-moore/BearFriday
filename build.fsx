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

    CopyDir buildDir "./client/dist" allFiles
)

Target "BuildServer" (fun _ ->
    DotNetCli.Build (fun p ->
        { p with
            WorkingDir = "./server" }
    )

    CopyDir buildDir "./server/bin/Release/net462" allFiles
)

Target "Run" (fun _ ->
    log (__SOURCE_DIRECTORY__ </> buildDir)
    let result = ExecProcess (fun info ->
        info.FileName <- (__SOURCE_DIRECTORY__ </> buildDir </> "BearFriday.Server.exe")
        info.WorkingDirectory <- (__SOURCE_DIRECTORY__ </> buildDir)) (TimeSpan.FromMinutes 10.0)

    if result <> 0 then failwithf "Server returned with a non-zero exit code"
)

Target "Default" DoNothing

"Clean"
    ==> "InstallPackages"
    ==> "BuildServer"
    ==> "BuildClient"
    ==> "Default"

RunTargetOrDefault "Default"