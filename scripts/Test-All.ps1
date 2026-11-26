param([switch]$Browser)
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$container = 'crisis-tests-' + [Guid]::NewGuid().ToString('N')
$password = [Guid]::NewGuid().ToString('N')
$previousConnection = $env:TEST_MARIADB_CONNECTION
$previousBrowser = $env:RUN_BROWSER_TESTS
$created = $false
Push-Location $repo
try {
    docker info --format '{{.ServerVersion}}'
    if ($LASTEXITCODE -ne 0) { throw 'Start Docker Desktop og kontroller Docker-tilgangen først.' }
    docker run --detach --rm --name $container --publish '127.0.0.1::3306' --env "MARIADB_ROOT_PASSWORD=$password" mariadb:11
    if ($LASTEXITCODE -ne 0) { throw 'Kunne ikke starte testdatabasen.' }
    $created = $true
    $ready = $false
    for ($attempt = 0; $attempt -lt 60; $attempt++) {
        docker exec $container healthcheck.sh --connect --innodb_initialized *> $null
        if ($LASTEXITCODE -eq 0) { $ready = $true; break }
        Start-Sleep -Seconds 1
    }
    if (-not $ready) { throw 'Testdatabasen ble ikke klar innen 60 sekunder.' }
    $details = docker inspect $container | ConvertFrom-Json
    $port = $details[0].NetworkSettings.Ports.'3306/tcp'[0].HostPort
    $env:TEST_MARIADB_CONNECTION = "Server=127.0.0.1;Port=$port;User=root;Password=$password"
    $env:RUN_BROWSER_TESTS = if ($Browser) { '1' } else { '0' }
    dotnet build CrisisSystem.Tests/CrisisSystem.Tests.csproj --configuration Release
    if ($LASTEXITCODE -ne 0) { throw 'Byggingen feilet.' }
    if ($Browser) {
        & ./CrisisSystem.Tests/bin/Release/net10.0/playwright.ps1 install chromium
        if ($LASTEXITCODE -ne 0) { throw 'Kunne ikke installere testnettleseren.' }
    }
    dotnet test CrisisSystem.Tests/CrisisSystem.Tests.csproj --configuration Release --no-build --no-restore --logger 'trx;LogFileName=tests.trx' --results-directory TestResults
    if ($LASTEXITCODE -ne 0) { throw 'En eller flere tester feilet. Se TestResults/tests.trx.' }
}
finally {
    if ($created) { docker rm --force $container | Out-Null }
    $env:TEST_MARIADB_CONNECTION = $previousConnection
    $env:RUN_BROWSER_TESTS = $previousBrowser
    Pop-Location
}
