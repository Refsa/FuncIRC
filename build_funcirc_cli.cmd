@echo off
cls

echo ####################################
echo ##### Building FuncIRC Library #####
echo ####################################
dotnet build FuncIRC/

echo ################################
echo ##### Building FuncIRC_CLI #####
echo ################################
dotnet build FuncIRC_CLI/