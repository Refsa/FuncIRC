@echo off
cls

dotnet build FuncIRC/
cls
dotnet run --project FuncIRC_CLI/