# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/BGME.MDMp3.API/*" -Force -Recurse
dotnet publish "./BGME.MDMp3.API.csproj" -c Release -o "$env:RELOADEDIIMODS/BGME.MDMp3.API" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location