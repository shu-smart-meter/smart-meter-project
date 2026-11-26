# Smart Meter Monitoring Dashboards

## Overview

This frontend uses Grafana to monitor electricity consumption, billing and grid alerts from the Smart Meter distributed application.

Grafana communicates with the C#/.NET backend through REST API endpoints using the Infinity data source plugin.

The system includes two dashboards:

- **Smart Meter Monitoring** — Displays readings, billing information and grid alerts for individual smart meters.
- **Smart Meter Comparison** — Displays electricity consumption from 12 smart meters simultaneously, allowing comparisons between meters.

Both dashboards are automatically provisioned through Docker Compose.

## Requirements

- Docker Desktop with Docker Compose
- Git
- .NET 10 SDK (for building and running Smart Meter clients locally)
- Smart Meter project repository

## Running the Dashboards

From the project's root directory, run:

```powershell
docker compose up -d --build
```

Once the containers are running, open:

http://localhost:3000

For local development, the initial Grafana credentials are:

- Username: `admin`
- Password: `admin`

Change the default password after logging in.

Navigate to **Dashboards** to access:

- Smart Meter Monitoring
- Smart Meter Comparison

The dashboards and Infinity data source are automatically provisioned from the repository.

**Important:** The backend and Grafana containers must be running for the dashboards to retrieve data. Smart Meter clients must also be running and sending readings.

## Dashboard Features

### Smart Meter Monitoring

This dashboard allows users to monitor individual smart meters.

**Electricity Consumption**

Displays electricity consumption readings over time for the selected meter.

**Live Electricity Bill**

Displays the server-calculated electricity bill in GBP (£).

**Grid Alerts**

Displays electricity grid warnings retrieved from the backend.

**Smart Meter Selection**

A dynamic dropdown retrieves registered meter IDs from the backend and allows users to switch between meters.

### Smart Meter Comparison

This dashboard displays electricity consumption from 12 smart meters simultaneously.

Each meter is represented by a separate line on the graph, allowing users to compare electricity usage across multiple clients.

The dashboard uses the following predefined meter IDs:

`METER001` through `METER012`

**Important:** Unlike Smart Meter Monitoring, the comparison dashboard uses predefined meter IDs rather than a dynamic dropdown.

The corresponding clients must be running and sending readings for their data to appear.

Both dashboards refresh periodically to display new readings.

## Architecture

```text
Smart Meter Clients
        |
        | SignalR
        v
.NET Smart Meter Server
        |
        | REST API (HTTP/JSON)
        v
Grafana Infinity Data Source
        |
        +-----------------------------+
        |                             |
        v                             v
Smart Meter Monitoring     Smart Meter Comparison
```

The Smart Meter clients communicate directly with the backend using SignalR.

Grafana retrieves information from the backend through REST API requests.

Grafana refreshes its data every 10 seconds.

The simulated smart meter clients operate independently of Grafana.

## Configuration

The relevant configuration files are:

- `compose.yaml` — Docker services, networking and volume mappings.
- `provisioning/datasources/datasource.yaml` — API connection and authentication.
- `provisioning/dashboards/dashboard.yaml` — Automatic dashboard provisioning.
- `smart-meter-dashboard.json` — Individual monitoring dashboard layout and queries.
- `smart-meter-comparison.json` — Comparison dashboard containing 12 predefined meter queries (`METER001`–`METER012`).

The backend admin token is supplied through the `SMART_METER_ADMIN_TOKEN` environment variable.

The default token is intended for local development only and must not be used in production.

## Testing

The dashboards have been manually tested with 12 concurrently running smart meter clients.

The following functionality has been verified locally:

- Smart meter selection
- Electricity consumption graphs
- Live electricity billing
- Grid alert retrieval
- Automatic dashboard provisioning
- REST API authentication
- Simultaneous visualisation of 12 meters
- Dashboard loading after restarting Grafana

### Testing Smart Meter Monitoring

1. Start Docker using the command provided in the Running the Dashboards section.
2. Start at least one Smart Meter client.
3. Open Grafana at `http://localhost:3000`.
4. Navigate to **Smart Meter Monitoring**.
5. Select an available meter from the dropdown.
6. Confirm electricity consumption readings are displayed.
7. Confirm the electricity bill is displayed in GBP.
8. Confirm grid alerts can be retrieved when available.
9. Check that the dashboard updates as new readings arrive.

### Testing Smart Meter Comparison

The comparison dashboard requires 12 clients using the exact naming convention `METER001` through `METER012`.

From the project root directory, build the client:

```powershell
dotnet build src/SmartMeter.Client -c Release
```

Then launch all 12 clients:

```powershell
1..12 | ForEach-Object {
    $meterId = "METER{0:D3}" -f $_
    Start-Process -FilePath "dotnet" `
        -ArgumentList "src/SmartMeter.Client/bin/Release/net10.0/SmartMeter.Client.dll", $meterId `
        -WorkingDirectory (Get-Location).Path
}
```

This starts 12 separate client processes.

Open **Smart Meter Comparison** in Grafana.

Verify that:

- All 12 meters appear in the graph legend.
- Each meter displays electricity consumption readings.
- Each meter is labelled correctly.
- The graph updates as new readings arrive.
- The dashboard remains accessible after restarting Grafana.

Allow time for the clients to generate their first readings.

**Important:** The comparison dashboard uses predefined meter IDs. If clients use a different naming convention, the graph may display "No data".

### Testing Automatic Provisioning

To test dashboard provisioning:

1. Ensure both dashboards are available in Grafana.
2. Restart Grafana using:

```powershell
docker compose restart grafana
```

3. Refresh the Grafana browser page.
4. Confirm both dashboards are still available.
5. Confirm the monitoring dashboard works.
6. Confirm the comparison dashboard displays readings for the running `METER001`–`METER012` clients.

### Troubleshooting

**Comparison dashboard displays "No data"**

- Confirm that Smart Meter clients are running.
- Confirm they use the exact IDs `METER001` through `METER012`.
- Confirm the backend is running.
- Allow time for readings to be generated.
- Refresh the dashboard and check the selected time range.

**Grafana cannot retrieve API data**

- Confirm the Smart Meter Server container is running.
- Confirm the Infinity data source is configured correctly.
- Check that the backend authentication token matches the configured development token.

**Dashboards do not appear**

- Confirm Docker Compose started Grafana successfully.
- Check that the dashboard JSON files are available through the configured Docker volume mappings.
- Verify the dashboard provisioning configuration.

## Limitations

- Grafana uses periodic polling rather than receiving SignalR push events directly.
- Both dashboards depend on the backend being available.
- The comparison dashboard uses 12 predefined meter IDs rather than dynamically discovering all available meters.
- The dashboards are designed for local development and demonstration.
- The system currently uses HTTP for local communication; production deployment would require HTTPS and stronger credential management.

## Shutting Down

Close the running Smart Meter client windows.

From the project root directory, run:

```powershell
docker compose down
```

This stops the Docker services without deleting their persistent volumes.

Avoid using `docker compose down -v` unless you intentionally want to remove Docker-managed volumes and their stored data.