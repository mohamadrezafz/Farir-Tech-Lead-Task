# Tech-Lead Task: Producer, Solution, Echo

## Task Description

This repository simulates a traffic enforcement pipeline. Three services work together:

- **Producer** (.NET 8): Generates fake traffic data and POSTs it as JSON events to a list of webhook URLs. You do not implement the Producer; it is provided.
- **Solution**: The service you implement. It receives the same events via webhook, persists them (cars, speed readings, tickets), and exposes APIs for querying tickets and car speed statistics.
- **Echo**: A small service that receives the same webhook calls as the Solution and logs **only the date and body** of each request to the console. Use the Echo logs to see exactly which events the Producer is sending (useful for debugging and understanding the stream of data).

**Objects and events**

- **Cars**: The Producer has a fixed pool of 20 cars (`carId`, `plateNumber`). It sends **CarGenerated** events; the same car can appear many times (duplicates). The Solution should treat these as "car registered" and store/deduplicate so each car is represented once.
- **Speed readings**: For each car, the Producer sends **SpeedRecorded** events (speed 70–130 km/h, `cameraId`, `timestamp`). The Solution should store these. If **speed > 110**, the Solution must create a **speed ticket** and store it.
- **Manual tickets**: The Producer also sends **ManualTicket** events (police-issued tickets for other offenses: `ticketId`, `carId`, `reason`, `timestamp`). The Solution should store these as tickets.

So **tickets** in your system come from: (1) tickets you create when a speed reading is > 110, and (2) manual tickets received from the Producer.

## Acceptance Criteria

- **Producer**: Generates CarGenerated, SpeedRecorded, and ManualTicket; sends each event to every configured webhook URL; runs in Docker.
- **Solution**: Receives webhook events; stores cars (with deduplication), speed readings, and tickets; creates a ticket when SpeedRecorded has speed > 110; stores ManualTicket events as tickets; exposes the two APIs described in [src/Solution/README.md](src/Solution/README.md) (list tickets with date filter; car speed stats for latest X minutes). You may use any SQL database you want (not required to use in-memory only).
- **Echo**: Receives the same webhook calls and logs date + body to the console, so you can see the events the Producer is sending.
- **Docker Compose**: All three services run via `docker compose up`; Producer's webhook URLs are configured in `docker-compose.yml` so it sends to both Solution and Echo.

## Configuration

Webhook URLs and the producer interval are configured in **docker-compose.yml** under the `producer` service (e.g. `Webhooks__Urls` and `Producer__IntervalSeconds`). You can change these to add or remove webhook targets or adjust how often events are sent.

| Variable | Description |
|----------|-------------|
| **Webhooks__Urls** | Comma-separated webhook URLs the Producer POSTs each event to (default in compose: Solution and Echo). |
| **Producer__IntervalSeconds** | Seconds between each batch of events (default: 2). |

## How to Run

### Using Docker Compose (recommended)

```bash
docker compose up --build
```

- **Solution**: http://localhost:8081  
- **Echo**: http://localhost:8082  
- **Producer**: runs in the background and sends events to both Solution and Echo. Webhook URLs are set in `docker-compose.yml` (see `Webhooks__Urls` under the `producer` service).

### Running locally

1. **Echo**: `cd src/Echo && dotnet run` (listens on port 8082 by default).
2. **Solution**: `cd src/Solution && dotnet run` (listens on port 8081 by default).
3. **Producer**: Set `Webhooks__Urls` (e.g. `http://localhost:8081/webhook,http://localhost:8082/webhook`), then `cd src/Producer && dotnet run`.

See [src/Solution/README.md](src/Solution/README.md) for the candidate task description (what to implement in the Solution).
