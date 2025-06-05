param(
    [string]$AppName
)

if (-not $AppName) {
    Write-Host "AppName parameter is required."
    exit 1
}

if (Test-Path -Path "$env:LOCALAPPDATA\$AppName\HDS") {
    if (Test-Path -Path "$env:TEMP\HDS") {
        Remove-Item -Path "$env:TEMP\HDS" -Recurse -Force
    }
    Copy-Item -Path $env:LOCALAPPDATA\$AppName\HDS -Destination $env:TEMP\HDS -Recurse -Force

    . "$env:TEMP\HDS\HDS.exe" --uninstall --app-name="$AppName"
} else {
    Write-Host "HDS directory not found in $env:LOCALAPPDATA\$AppName."
    exit 1
}