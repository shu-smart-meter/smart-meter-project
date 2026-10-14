# smart-meter-project

Simulation of a smart meter system for recording and managing domestic electricity consumption.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Optional: Visual Studio 2022, or VS Code with the C# Dev Kit extension

## Getting started

Open `SmartMeter.sln` in Visual Studio or VS Code, or use the .NET CLI from the repository root.

Build:

```sh
dotnet build
```

Run the tests:

```sh
dotnet test
```

Run the server:

```sh
dotnet run --project src/SmartMeter.Server
```

The server listens on `http://localhost:5080`.

## Run with Docker

```sh
docker compose up --build
```

The server is then available on `http://localhost:5080`.

### Environment variables

Compose supplies local defaults, so the server can start without a `.env` file. Override these values in the shell
or in a `.env` file beside `compose.yaml`:

- `SMART_METER_CLIENT_SECRET` defaults to `local-development-client-secret`. Clients send this value to request a meter token.
- `SMART_METER_ADMIN_TOKEN` defaults to `local-development-admin-token`. Use this bearer token for admin-only endpoints.
- `Billing__UnitPricePerKwh` defaults to `0.24`. This is the price per kWh used to calculate bills.
- `SMART_METER_TOKEN_SECONDS` defaults to `3600`. This sets the lifetime of a meter token in seconds.
- `Storage__Path` is set to `/data/readings.json` inside the Compose named volume. It does not need a host environment variable.

The default credentials are for local development. Set your own values before you expose the server to other people
or networks. A client must use the same `SMART_METER_CLIENT_SECRET` value as the server.

To override a value from the shell before starting Compose:

```sh
export SMART_METER_CLIENT_SECRET='choose-a-local-secret'
export SMART_METER_ADMIN_TOKEN='choose-a-local-admin-token'
export Billing__UnitPricePerKwh=0.30
export SMART_METER_TOKEN_SECONDS=1800
docker compose up --build
```
