@echo off

REM Vars
set "SLNDIR=%~dp0src"

REM Restore + Build
dotnet build "%SLNDIR%\Yearl.sln" --nologo || exit /b

REM Test
dotnet test "%SLNDIR%\Yearl.Tests" --nologo --no-build