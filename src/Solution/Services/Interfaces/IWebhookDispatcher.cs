namespace Solution.Services.Interfaces;

/// <summary>
/// Parses webhook JSON, validates event type, and dispatches to the appropriate handler.
/// Keeps Program.cs thin and makes the pipeline easy to unit test.
/// </summary>
public interface IWebhookDispatcher
{
    Task<Models.WebhookDispatchResult> DispatchAsync(string jsonBody, CancellationToken ct = default);
}
