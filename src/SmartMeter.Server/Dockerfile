FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# restore on its own first so docker caches it; only reruns when the csproj changes
COPY global.json Directory.Build.props ./
COPY src/SmartMeter.Api/SmartMeter.Api.csproj src/SmartMeter.Api/
COPY src/SmartMeter.Server/SmartMeter.Server.csproj src/SmartMeter.Server/
RUN dotnet restore src/SmartMeter.Server/SmartMeter.Server.csproj

COPY src/SmartMeter.Api/ src/SmartMeter.Api/
COPY src/SmartMeter.Server/ src/SmartMeter.Server/
RUN dotnet publish src/SmartMeter.Server/SmartMeter.Server.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER app

ENTRYPOINT ["dotnet", "SmartMeter.Server.dll"]
