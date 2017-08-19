param (
    [switch]$Release,
    [switch]$Run
)

$ErrorActionPreference = 'Stop'

function Invoke-Cmd ($cmd) {
    Write-Host $cmd -ForegroundColor DarkCyan
    $command = "cmd.exe /C $cmd"
    Invoke-Expression -Command $command
    if ($LastExitCode -ne 0) { Write-Error "An error occured when executing '$cmd'."; return }
}

function dotnet ($cmd, $argv) { Invoke-Cmd "dotnet $cmd $argv" }
function yarn ($cmd, $argv) { Invoke-Cmd "yarn $cmd $argv" }

$buildDir = '.\build'
$serverDir = '.\server'
$serverProj = "$serverDir\BearFriday.Server.fsproj"
$clientDir = '.\client'
$configuration = if ($Release) { 'Release' } else { 'Debug' }

dotnet --version

# clean build output
dotnet clean $serverProj
Get-ChildItem -Path  $buildDir -Include * | Remove-Item -Recurse 

# build projects
dotnet restore $serverProj
dotnet build $serverProj

Set-Location $clientDir
yarn install
yarn build
Set-Location '..'

# copy output to build dir
if(-not(Test-Path -Path $buildDir)) {
    New-Item -ItemType directory -Path $buildDir
}
Copy-Item "$serverDir\bin\$configuration\netcoreapp1.1\*" $buildDir -Recurse
@('css', 'dist', 'img') | % { 
    $outputDir = if ($_ -eq 'dist') { 'js' } else { $_ }
    Copy-Item "$clientDir\$_" "$buildDir\wwwroot\$outputDir" -Recurse  
}

if ($Run) {
    Set-Location $buildDir
    dotnet 'BearFriday.Server.dll'
    Set-Location '..'
}