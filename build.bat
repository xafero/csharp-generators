@echo off

dotnet clean
dotnet build
dotnet pack --configuration Release -o nupkg
