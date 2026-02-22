using Microsoft.EntityFrameworkCore;
using Solution.Data;
using Solution.Data.Entities;
using Solution.Models;

namespace Solution.Services;
public interface ITicketQueryService
{
    Task<TicketListResponse> GetTicketsAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken ct = default);
    Task<SpeedStatsResponse?> GetCarSpeedStatsAsync(string carId, int minutes, CancellationToken ct = default);
}
