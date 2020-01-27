@echo off
cls

dotnet build FuncIRC/

cd FuncIRC.Tests/

dotnet test
dotnet nbench --fx-version 2.1.13

cd ..