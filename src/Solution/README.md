# Solution – Candidate Task

This project receives webhook calls from the Producer. Implement the behavior and APIs described below.

## Objects and events

The Producer sends JSON events to `POST /webhook`. There are three event types; you should model and store the following:

| Event | Meaning | What to do |
|-------|---------|------------|
| **CarGenerated** | A car (from a fixed pool of 20) was "registered". The same car can be sent many times. | Store/deduplicate so you have one record per car (`carId`, `plateNumber`). |
| **SpeedRecorded** | A speed camera recorded a car's speed (70–130 km/h). | Store the reading. If **speed > 110**, create and store a **speed ticket** linked to that car/reading. |
| **ManualTicket** | Police issued a ticket for another offense (e.g. running a red light). | Store it as a ticket (you receive `ticketId`, `carId`, `reason`, `timestamp`). |

**Tickets** in your system therefore come from: (1) tickets you create when a speed reading is > 110, and (2) manual tickets from the Producer.

**Storage**: Use any SQL database you prefer (e.g. SQL Server, PostgreSQL, SQLite). You are not required to use in-memory storage only.

## Event payloads (reference)

- **CarGenerated**: `{ "event": "CarGenerated", "carId": "...", "plateNumber": "...", "timestamp": "..." }`
- **SpeedRecorded**: `{ "event": "SpeedRecorded", "carId": "...", "speed": 70–130, "cameraId": "...", "timestamp": "..." }`
- **ManualTicket**: `{ "event": "ManualTicket", "ticketId": "...", "carId": "...", "reason": "...", "timestamp": "..." }`

## Requirements

1. **Webhook**: Accept `POST /webhook` and handle the three event types above (store cars with deduplication, store speed readings, create speed tickets when speed > 110, store manual tickets).

2. **API – List tickets with date filter**: Expose an API that returns all tickets, with an optional date filter (e.g. from/to date or date range). Exact path and query parameters are up to you (e.g. `GET /api/tickets?from=...&to=...`).

3. **API – Car speed statistics**: Expose an API that, for a given car (e.g. by `carId`) and a time window (e.g. "latest X minutes"), returns the **average**, **max**, and **min** speed for that car in that window. Exact path and parameters are up to you (e.g. `GET /api/cars/{carId}/speed-stats?minutes=30` returning something like `{ "avg": 95, "max": 120, "min": 72 }`).

Implement the webhook handling and both APIs in this project. Use the SQL database of your choice for persistence.
