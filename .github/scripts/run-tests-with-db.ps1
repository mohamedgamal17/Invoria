Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$resultsRoot = Join-Path $PWD "TestResults"
New-Item -Path $resultsRoot -ItemType Directory -Force | Out-Null

$testProjects = Get-ChildItem -Path "tests" -Recurse -Filter "*.csproj" | Where-Object {
    Select-String -Path $_.FullName -Pattern "<IsTestProject>true</IsTestProject>" -Quiet
}

if (-not $testProjects) {
    throw "No test projects were found under tests/."
}

foreach ($testProject in $testProjects) {
    $projectName = [System.IO.Path]::GetFileNameWithoutExtension($testProject.Name)
    $safeName = ($projectName -replace "[^a-zA-Z0-9]", "").ToLowerInvariant()
    if ([string]::IsNullOrWhiteSpace($safeName)) {
        $safeName = "testproject"
    }

    $dbName = "ci_${safeName}_${env:GITHUB_RUN_ID}_${env:GITHUB_RUN_ATTEMPT}"
    $connectionString = "Server=localhost,1433;Database=$dbName;User Id=sa;Password=$env:SA_PASSWORD;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=True"
    $resultsDirectory = Join-Path $resultsRoot $safeName
    New-Item -Path $resultsDirectory -ItemType Directory -Force | Out-Null

    Write-Host "Running tests for $($testProject.FullName) using database $dbName"

    $env:ConnectionStrings__Default = $connectionString
    $env:ConnectionStrings__Rebus = $connectionString

    dotnet test $testProject.FullName `
        --configuration Release `
        --no-build `
        --logger "trx;LogFileName=$($safeName).trx" `
        --results-directory $resultsDirectory
}
