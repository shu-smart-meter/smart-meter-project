# smart-meter-project

Simulation of a smart meter system for recording and managing domestic electricity consumption.

## Requirements

- Windows 10 or 11 with PowerShell
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Optional: Visual Studio or VS Code with the C# Dev Kit extension
- Docker Desktop with Compose v2, if you want to run the container

## Getting started

Open PowerShell in the repository root. You can open `SmartMeter.sln` in Visual Studio or VS Code, or use these
commands.

Build the solution:

```powershell
dotnet build .\SmartMeter.sln
```

Run the tests:

```powershell
dotnet test .\SmartMeter.sln
```

Run the server:

```powershell
dotnet run --project .\src\SmartMeter.Server\SmartMeter.Server.csproj
```

The server listens on `http://localhost:5080`. Press `Ctrl+C` to stop it.

Direct runs do not use the Compose defaults. Set the client secret before starting the server if clients need to
request tokens. Set the admin token if you need to call admin-only endpoints:

```powershell
$env:SMART_METER_CLIENT_SECRET = "choose-a-local-secret"
$env:SMART_METER_ADMIN_TOKEN = "choose-a-local-admin-token"
dotnet run --project .\src\SmartMeter.Server\SmartMeter.Server.csproj
```

## Run with Docker

Start Docker Desktop, then run this from the directory that contains `compose.yaml`:

```powershell
docker compose up --build
```

The server is then available on `http://localhost:5080`. Press `Ctrl+C` to stop it. The named volume keeps readings
when you stop or rebuild the container.

### Environment variables

Compose supplies local defaults, so the server can start without a `.env` file. Override these values in PowerShell
or in a `.env` file beside `compose.yaml`:

- `SMART_METER_CLIENT_SECRET` defaults to `local-development-client-secret`. Clients send this value to request a meter token.
- `SMART_METER_ADMIN_TOKEN` defaults to `local-development-admin-token`. Use this bearer token for admin-only endpoints.
- `Billing__UnitPricePerKwh` defaults to `0.24`. This is the price per kWh used to calculate bills.
- `SMART_METER_TOKEN_SECONDS` defaults to `3600`. This sets the lifetime of a meter token in seconds.
- `Storage__Path` is set to `/data/readings.json` inside the Compose named volume. It does not need a host environment variable.

The default credentials are for local development. Set your own values before you expose the server to other people
or networks. A client must use the same `SMART_METER_CLIENT_SECRET` value as the server.

To override values in PowerShell before starting Compose:

```powershell
$env:SMART_METER_CLIENT_SECRET = "choose-a-local-secret"
$env:SMART_METER_ADMIN_TOKEN = "choose-a-local-admin-token"
$env:Billing__UnitPricePerKwh = "0.30"
$env:SMART_METER_TOKEN_SECONDS = "1800"
docker compose up --build
```

These values apply only to the current PowerShell window. To remove an override and use the Compose default, run
`Remove-Item Env:SMART_METER_CLIENT_SECRET` with the matching variable name.
