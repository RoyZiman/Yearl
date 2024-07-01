@echo off

dotnet build .\src\Yearl.sln /nologo
dotnet test .\src\Yearl.Tests\Yearl.Tests.csproj

@pause