param (
    [switch]$Release
    [switch]$Run
)

$ErrorActionPreference = 'Stop'

function Invoke-Cmd ($cmd) {
    Write-Host $cmd -ForegroundColor DarkCyan
    $command = "cmd.exe /C $cmd"
    Invoke-Expression -Command $command
    if ($LastExitCode -ne 0) { Write-Error "An error occured when executing '$cmd'."; return }
}

function dotnet ($assembly, $argv) { Invoke-Cmd "dotnet $assembly $argv" }
function dotnet-restore ($project, $argv) { Invoke-Cmd "dotnet restore $project $argv" }
function dotnet-build   ($project, $argv) { Invoke-Cmd "dotnet build $project $argv" }
function dotnet-run     ($project, $argv) { Invoke-Cmd "dotnet run --project $project $argv" }
function dotnet-test    ($project, $argv) { Invoke-Cmd "dotnet test $project $argv" }
function dotnet-pack    ($project, $argv) { Invoke-Cmd "dotnet pack $project $argv" }
function dotnet-clean   ($project, $argv) { Invoke-Cmd "dotnet clean $project $argv" }

$buildDir = '.\build'
$serverDir = '.\server'
$serverProj = "$serverDir\BearFriday.Server.fsproj"
$clientDir = '.\client'
$configuration = if ($Release) { 'Release' } else { 'Debug' }

Invoke-Cmd 'dotnet --version'

# clean build output
dotnet-clean $serverProj
Get-ChildItem -Path  $buildDir -Include * | Remove-Item -Recurse 

# build projects
dotnet-restore $serverProj
dotnet-build $serverProj

# copy output to build dir
if(-not(Test-Path -Path $buildDir)) {
    New-Item -ItemType directory -Path $buildDir
}
Copy-Item "$serverDir\bin\$configuration\netcoreapp1.1\*" $buildDir -Recurse

if ($Run) {
    dotnet "$buildDir\BearFriday.Server.dll"
}