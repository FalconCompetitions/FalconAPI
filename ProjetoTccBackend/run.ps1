#!/usr/bin/env pwsh

Write-Host "Select the project Environment:"
Write-Host "1 - Development"
Write-Host "2 - Staging"
Write-Host "3 - Production"
$option = Read-Host

if ($option -eq "1") {
    $env:ASPNETCORE_ENVIRONMENT = "Development"
}
elseif ($option -eq "2") {
    $env:ASPNETCORE_ENVIRONMENT = "Staging"
}
elseif ($option -eq "3") {
    $env:ASPNETCORE_ENVIRONMENT = "Production"
}
else {
    Write-Host "Invalid option"
    exit 1
}

$ASPNETCORE_PROFILE = ""

if ($env:ASPNETCORE_ENVIRONMENT -eq "Development") {
    $ASPNETCORE_PROFILE = "Debug"
}
elseif ($env:ASPNETCORE_ENVIRONMENT -eq "Staging") {
    $ASPNETCORE_PROFILE = "Production"
}
elseif ($env:ASPNETCORE_ENVIRONMENT -eq "Production") {
    $ASPNETCORE_PROFILE = "Production"
}

dotnet run -lp https --no-self-contained -c $ASPNETCORE_PROFILE
