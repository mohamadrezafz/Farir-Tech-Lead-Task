using Solution.Models;

namespace Solution.Services.Interfaces;

public interface IWebhookHandlerService
{
    Task ProcessCarGeneratedAsync(CarGeneratedPayload payload, CancellationToken ct = default);
    Task ProcessSpeedRecordedAsync(SpeedRecordedPayload payload, CancellationToken ct = default);
    Task ProcessManualTicketAsync(ManualTicketPayload payload, CancellationToken ct = default);
}