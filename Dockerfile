FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY vs2022/FMP/bin/Release/net7.0/publish/ ./
ENTRYPOINT ["dotnet", "FMP.dll"]
