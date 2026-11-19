
# Smart Meter Monitoring Dashboard

## Overview

This frontend uses Grafana to monitor electricity consumption, billing and grid alerts from the Smart Meter distributed application.

Grafana communicates with the C#/.NET backend through REST API endpoints using the Infinity data source plugin.

## Requirements

- Docker Desktop with Docker Compose
- Git
- Smart Meter project repository

## Running the Dashboard

From the project's root directory, run:

```powershell
docker compose up -d --build
```

Once the containers are running, open:

http://localhost:3000

The Grafana dashboard is automatically provisioned from the repository.

For local development, the initial Grafana credentials are:

- Username: admin
- Password: admin

Change the default password after logging in.

## Dashboard Features

### Electricity Consumption
Displays electricity readings over time for the selected smart meter.

### Live Electricity Bill
Displays the server-calculated electricity bill in GBP (£).

### Grid Alerts
Displays electricity grid warnings retrieved from the backend.

### Smart Meter Selection
A dynamic dropdown retrieves registered meter IDs from the backend and allows users to switch between meters.

## Architecture

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
        v
Smart Meter Monitoring Dashboard

Grafana refreshes its data every 10 seconds.

The simulated smart meter clients operate independently of Grafana and communicate directly with the backend.

## Configuration

- compose.yaml: Docker services
- provisioning/datasources/datasource.yaml: API connection and authentication
- provisioning/dashboards/dashboard.yaml: Dashboard provisioning
- smart-meter-dashboard.json: Dashboard layout and queries

The backend admin token is supplied through the SMART_METER_ADMIN_TOKEN environment variable.

The default token is intended for local development only and must not be used in production.

## Testing

The dashboard has been manually tested with 12 concurrently running smart meter clients.

The following functionality has been verified:

- Meter selection
- Electricity consumption graphs
- Live billing information
- Grid alert retrieval
- Automatic dashboard provisioning
- REST API authentication

## Limitations

- Grafana uses periodic polling rather than receiving SignalR push events directly.
- The dashboard depends on the backend being available.
- The dashboard is designed for local development and demonstration.
- The system currently uses HTTP for local communication; production deployment would require HTTPS and stronger credential management.
