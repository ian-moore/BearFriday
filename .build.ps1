param(
    $Configuration = 'Debug'
)

$azureDir = './src/azure'
$azureProj = "$azureDir/BearFriday.Azure.fsproj"

task CleanAzure {
    exec { & dotnet clean $azureProj }
}

task BuildAzure CleanAzure, {
    exec { & dotnet build $azureProj }
}

task PublishAzure CleanAzure, {
    exec { & dotnet publish $azureProj --configuration $Configuration }
}

task RunAzure PublishAzure, {
    Set-Location "$azureDir/bin/$Configuration/netstandard2.0/publish"
    exec { & func start }
}

task . BuildAzure