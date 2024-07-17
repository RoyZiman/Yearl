@echo off

REM Vars
set "SLNDIR=%~dp0src"

REM Restore + Build
dotnet build "%SLNDIR%\msc" --nologo || exit /b

REM Run
dotnet run --project "%SLNDIR%\msc" --no-build -- %*