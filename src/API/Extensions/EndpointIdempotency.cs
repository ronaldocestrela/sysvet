namespace API.Extensions;

internal static class EndpointIdempotency
{
    internal static Guid ReadKey(HttpContext context)
    {
        var headerValue = context.Request.Headers["Idempotency-Key"].FirstOrDefault();
        return Guid.TryParse(headerValue, out var key) ? key : Guid.Empty;
    }
}
