using Microsoft.EntityFrameworkCore;
using Solution.Data;
using Solution.Data.Entities;
using Solution.Models;

namespace Solution.Services;

public class TicketQueryService : ITicketQueryService
{
    private readonly IDbContextFactory<SolutionDbContext> _dbFactory;

    public TicketQueryService(IDbContextFactory<SolutionDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<TicketListResponse> GetTicketsAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var query = db.Tickets
            .Include(t => t.Car)
            .Include(t => t.SpeedReading)
            .AsNoTracking();

        if (fromUtc.HasValue)
            query = query.Where(t => t.RecordedAtUtc >= fromUtc.Value);
        if (toUtc.HasValue)
            query = query.Where(t => t.RecordedAtUtc <= toUtc.Value);

        var list = await query
            .OrderBy(t => t.RecordedAtUtc)
            .Select(t => new TicketDto(
                t.Id,
                t.Type == TicketType.Speed ? "Speed" : "Manual",
                t.Car.CarId,
                t.RecordedAtUtc,
                t.SpeedReading != null ? t.SpeedReading.SpeedKmh : null,
                t.Reason,
                t.ExternalTicketId))
            .ToListAsync(ct);

        return new TicketListResponse(list);
    }

    public async Task<SpeedStatsResponse?> GetCarSpeedStatsAsync(string carId, int minutes, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var car = await db.Cars.AsNoTracking().FirstOrDefaultAsync(c => c.CarId == carId, ct);
        if (car is null)
            return null;

        var since = DateTime.UtcNow.AddMinutes(-minutes);
        var readings = await db.SpeedReadings
            .AsNoTracking()
            .Where(r => r.CarId == car.Id && r.RecordedAtUtc >= since)
            .Select(r => r.SpeedKmh)
            .ToListAsync(ct);

        if (readings.Count == 0)
            return new SpeedStatsResponse(0, 0, 0);

        return new SpeedStatsResponse(
            Math.Round(readings.Average(), 2),
            readings.Max(),
            readings.Min());
    }
}

