namespace Solution.Models;

/// <summary>
/// Result of processing a webhook request. Used to keep HTTP concerns out of the dispatcher.
/// </summary>
public record WebhookDispatchResult(bool Success, string? ErrorMessage, int StatusCode)
{
    public static WebhookDispatchResult Ok() => new(true, null, 200);
    public static WebhookDispatchResult BadRequest(string message) => new(false, message, 400);
    public static WebhookDispatchResult ServerError(string message) => new(false, message, 500);
}
