param(
    $Configuration = 'Debug'
)

$azureDir = './src/azure'
$azureProj = "$azureDir/BearFriday.Azure.fsproj"
$azureTestProj = './test/azure/BearFriday.Azure.Test.fsproj'
$clientBuildDir = './dist'

task CleanAzure {
    exec { dotnet clean $azureProj }
}

task BuildAzure CleanAzure, {
    exec { dotnet build $azureProj }
}

task TestAzure {
    exec { dotnet test $azureTestProj }
}

task PublishAzure CleanAzure, {
    exec { dotnet publish $azureProj --configuration $Configuration }
}

task RunAzure PublishAzure, {
    Set-Location "$azureDir/bin/$Configuration/netstandard2.0/publish"
    exec { & func start }
}

task BuildClient {
    Remove-Item $clientBuildDir -Recurse
    if (-Not (Test-Path -Path $clientBuildDir)) {
        New-Item -ItemType directory -Path $clientBuildDir
    }

    exec { npm run build }
}

task DeployClient BuildClient, {
    Set-Location $clientBuildDir 

    git init
    git checkout -b gh-pages
    git remote add origin https://github.com/ian-moore/BearFriday.git
    git add .
    git commit -m "publish"
    git push -f origin gh-pages
}

task test TestAzure

task . BuildAzure