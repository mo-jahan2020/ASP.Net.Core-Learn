namespace SsoServer.Web.OpenIddict;

/// <summary>Immutable view of the token-pruning background service, consumed by the health page.</summary>
/// <param name="ServiceStartedAtUtc">When the background service started.</param>
/// <param name="LastRunCompletedAtUtc">When the last pruning iteration finished (null = not yet).</param>
/// <param name="NextRunExpectedAtUtc">When the next pruning iteration is expected.</param>
/// <param name="LastPrunedTokens">Tokens removed by the last iteration.</param>
/// <param name="LastPrunedAuthorizations">Authorizations removed by the last iteration.</param>
/// <param name="TotalRuns">Successful iterations since startup.</param>
/// <param name="LastError">Message of the last failed iteration, if any.</param>
/// <param name="LastErrorAtUtc">When the last failure occurred.</param>
public sealed record TokenPruningSnapshot(
    DateTimeOffset? ServiceStartedAtUtc,
    DateTimeOffset? LastRunCompletedAtUtc,
    DateTimeOffset? NextRunExpectedAtUtc,
    long LastPrunedTokens,
    long LastPrunedAuthorizations,
    int TotalRuns,
    string? LastError,
    DateTimeOffset? LastErrorAtUtc);

/// <summary>
/// Thread-safe, singleton status of <see cref="TokenPruningService"/> so the admin health page can
/// show a live green/red indicator for the background cleanup job without touching its internals.
/// </summary>
public sealed class TokenPruningStatus
{
    private readonly object _lock = new();

    private DateTimeOffset? _serviceStartedAtUtc;
    private DateTimeOffset? _lastRunCompletedAtUtc;
    private DateTimeOffset? _nextRunExpectedAtUtc;
    private long _lastPrunedTokens;
    private long _lastPrunedAuthorizations;
    private int _totalRuns;
    private string? _lastError;
    private DateTimeOffset? _lastErrorAtUtc;

    /// <summary>Called once when the hosted service starts.</summary>
    public void RecordStarted()
    {
        lock (_lock)
        {
            _serviceStartedAtUtc = DateTimeOffset.UtcNow;
        }
    }

    /// <summary>Called after every successful pruning iteration.</summary>
    public void RecordSuccess(long prunedTokens, long prunedAuthorizations, DateTimeOffset nextRunExpectedAtUtc)
    {
        lock (_lock)
        {
            _lastRunCompletedAtUtc = DateTimeOffset.UtcNow;
            _nextRunExpectedAtUtc = nextRunExpectedAtUtc;
            _lastPrunedTokens = prunedTokens;
            _lastPrunedAuthorizations = prunedAuthorizations;
            _totalRuns++;
            _lastError = null;
            _lastErrorAtUtc = null;
        }
    }

    /// <summary>Called when a pruning iteration throws; the error stays visible until the next success.</summary>
    public void RecordFailure(string error)
    {
        lock (_lock)
        {
            _lastError = error;
            _lastErrorAtUtc = DateTimeOffset.UtcNow;
        }
    }

    /// <summary>Copies the current values into an immutable snapshot (safe for concurrent readers).</summary>
    public TokenPruningSnapshot Snapshot()
    {
        lock (_lock)
        {
            return new TokenPruningSnapshot(
                _serviceStartedAtUtc,
                _lastRunCompletedAtUtc,
                _nextRunExpectedAtUtc,
                _lastPrunedTokens,
                _lastPrunedAuthorizations,
                _totalRuns,
                _lastError,
                _lastErrorAtUtc);
        }
    }
}
