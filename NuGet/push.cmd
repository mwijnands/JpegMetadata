cls
@echo off
set /p apiKey=NuGet API Key: 
nuget push *.nupkg %apiKey%
nuget push *.symbols.nupkg %apiKey%
pause
