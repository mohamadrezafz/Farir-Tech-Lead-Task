using Microsoft.EntityFrameworkCore;
using Solution.Data.Entities;
using Solution.Data;
using Solution.Models;
using Solution.Services.Interfaces;

namespace Solution.Services;


public class WebhookHandlerService : IWebhookHandlerService
{
    private readonly IDbContextFactory<SolutionDbContext> _dbFactory;
    private readonly int _speedLimitKmh;

    public WebhookHandlerService(IDbContextFactory<SolutionDbContext> dbFactory, IConfiguration configuration)
    {
        _dbFactory = dbFactory;
        _speedLimitKmh = configuration.GetValue("SpeedLimit:Kmh", 110);
    }

    public async Task ProcessCarGeneratedAsync(CarGeneratedPayload payload, CancellationToken ct = default)
    {
        var ts = ParseTimestamp(payload.Timestamp);
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var car = await db.Cars.FirstOrDefaultAsync(c => c.CarId == payload.CarId, ct);
        if (car is null)
        {
            car = new Car
            {
                CarId = payload.CarId,
                PlateNumber = payload.PlateNumber,
                CreatedAtUtc = ts
            };
            db.Cars.Add(car);
        }
        else
        {
            car.PlateNumber = payload.PlateNumber;
        }
        await db.SaveChangesAsync(ct);
    }

    public async Task ProcessSpeedRecordedAsync(SpeedRecordedPayload payload, CancellationToken ct = default)
    {
        var ts = ParseTimestamp(payload.Timestamp);
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var car = await db.Cars.FirstOrDefaultAsync(c => c.CarId == payload.CarId, ct);
        if (car is null)
        {
            car = new Car { CarId = payload.CarId, PlateNumber = "(unknown)", CreatedAtUtc = ts };
            db.Cars.Add(car);
            await db.SaveChangesAsync(ct);
        }
        var reading = new SpeedReading
        {
            CarId = car.Id,
            SpeedKmh = payload.Speed,
            CameraId = payload.CameraId,
            RecordedAtUtc = ts
        };
        db.SpeedReadings.Add(reading);
        await db.SaveChangesAsync(ct);
        if (payload.Speed > _speedLimitKmh)
        {
            var ticket = new Ticket
            {
                Type = TicketType.Speed,
                CarId = car.Id,
                RecordedAtUtc = ts,
                SpeedReadingId = reading.Id
            };
            db.Tickets.Add(ticket);
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task ProcessManualTicketAsync(ManualTicketPayload payload, CancellationToken ct = default)
    {
        var ts = ParseTimestamp(payload.Timestamp);
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var exists = await db.Tickets.AnyAsync(t => t.ExternalTicketId == payload.TicketId, ct);
        if (exists) return;

        var car = await db.Cars.FirstOrDefaultAsync(c => c.CarId == payload.CarId, ct);
        if (car is null)
        {
            car = new Car { CarId = payload.CarId, PlateNumber = "(unknown)", CreatedAtUtc = ts };
            db.Cars.Add(car);
            await db.SaveChangesAsync(ct);
        }

        var ticket = new Ticket
        {
            Type = TicketType.Manual,
            CarId = car.Id,
            RecordedAtUtc = ts,
            ExternalTicketId = payload.TicketId,
            Reason = payload.Reason
        };
        db.Tickets.Add(ticket);
        await db.SaveChangesAsync(ct);
    }

    private static DateTime ParseTimestamp(string timestamp)
    {
        return DateTime.TryParse(timestamp, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt)
            ? DateTime.SpecifyKind(dt, DateTimeKind.Utc)
            : DateTime.UtcNow;
    }
}
