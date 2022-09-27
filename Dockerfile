FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY vs2022/FMP/bin/Release/net6.0/publish/ ./
ENTRYPOINT ["dotnet", "FMP.dll"]
