using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;

namespace SsoServer.Web.OpenIddict;

/// <summary>
/// Prunes orphaned/expired OpenIddict tokens and authorizations every six hours so the
/// database does not grow unbounded (lightweight alternative to the Quartz integration).
/// </summary>
public sealed class TokenPruningService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);

    // Short warm-up so the admin health page shows a real "last run" shortly after startup
    // instead of staying empty for the first six hours. Pruning only removes entries older
    // than 7 days, so an early run is harmless.
    private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(30);

    private readonly IServiceProvider _provider;
    private readonly ILogger<TokenPruningService> _logger;
    private readonly TokenPruningStatus _status;

    public TokenPruningService(
        IServiceProvider provider,
        ILogger<TokenPruningService> logger,
        TokenPruningStatus status)
    {
        _provider = provider;
        _logger = logger;
        _status = status;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _status.RecordStarted();
        var delay = InitialDelay;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(delay, stoppingToken);
                delay = Interval;
                using var scope = _provider.CreateScope();
                var threshold = DateTimeOffset.UtcNow.AddDays(-7);
                var tokens = scope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();
                var authorizations = scope.ServiceProvider.GetRequiredService<IOpenIddictAuthorizationManager>();
                var prunedTokens = await tokens.PruneAsync(threshold, stoppingToken);
                var prunedAuthorizations = await authorizations.PruneAsync(threshold, stoppingToken);
                _status.RecordSuccess(prunedTokens, prunedAuthorizations, DateTimeOffset.UtcNow.Add(Interval));
                if (prunedTokens > 0 || prunedAuthorizations > 0)
                {
                    _logger.LogInformation("Pruned {Tokens} tokens and {Authorizations} authorizations", prunedTokens, prunedAuthorizations);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token pruning iteration failed");
                _status.RecordFailure(ex.Message);
            }
        }
    }
}
