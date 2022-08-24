FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY vs2022/_publish/ ./
ENTRYPOINT ["dotnet", "FMP.dll"]
