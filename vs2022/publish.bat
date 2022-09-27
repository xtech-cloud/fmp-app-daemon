@echo off

for /F %%i in ('git tag --contains') do ( set VERSION=%%i)
IF "%VERSION%" ==""  (
    echo [31m tag is required! [0m
    pause
    EXIT
)

dotnet publish -c Release /p:Version=%VERSION%
