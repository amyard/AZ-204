namespace HttpClientVersions.DelegatingHandler;

public class LoggingHandler : System.Net.Http.DelegatingHandler
{
    private readonly ILogger<LoggingHandler> _logger;

    public LoggingHandler(ILogger<LoggingHandler> logger)
    {
        _logger = logger;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation($"Before HTTP request: {request.RequestUri}");
            var result = await base.SendAsync(request, cancellationToken);
            result.EnsureSuccessStatusCode();
            _logger.LogInformation($"After HTTP request: {request.RequestUri}");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Http request failed.");
            throw;
        }
    }
}
