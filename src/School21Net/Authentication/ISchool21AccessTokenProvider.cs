namespace School21Net.Authentication;

/// <summary>
/// Supplies a valid bearer access token to <see cref="School21Client"/> on each request. The SDK never
/// caches or refreshes tokens itself — implement this on the integrator side to own the token lifecycle
/// (cache the current token, refresh it via <see cref="School21AuthClient"/> before it expires, and
/// re-authenticate when the refresh token is gone). Implementations must be safe for concurrent calls.
/// </summary>
public interface ISchool21AccessTokenProvider
{
    /// <summary>Return a currently valid access token, refreshing on the integrator side if needed.</summary>
    ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken);
}

/// <summary>
/// An <see cref="ISchool21AccessTokenProvider"/> that always returns the same token. Suitable for scripts or
/// short-lived operations where the token outlives the work; it does not refresh.
/// </summary>
public sealed class StaticAccessTokenProvider : ISchool21AccessTokenProvider
{
    private readonly string _accessToken;

    /// <summary>Create a provider that always returns <paramref name="accessToken"/>.</summary>
    public StaticAccessTokenProvider(string accessToken)
        => _accessToken = School21WireParsing.RequireNonEmpty(accessToken, nameof(accessToken));

    /// <inheritdoc />
    public ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken)
        => ValueTask.FromResult(_accessToken);
}

/// <summary>
/// An <see cref="ISchool21AccessTokenProvider"/> that delegates to a caller-supplied function. Lets an
/// integrator plug in a refresh-aware accessor without writing a dedicated class.
/// </summary>
public sealed class DelegateAccessTokenProvider : ISchool21AccessTokenProvider
{
    private readonly Func<CancellationToken, ValueTask<string>> _accessor;

    /// <summary>Create a provider backed by <paramref name="accessor"/>.</summary>
    public DelegateAccessTokenProvider(Func<CancellationToken, ValueTask<string>> accessor)
        => _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));

    /// <inheritdoc />
    public ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken)
        => _accessor(cancellationToken);
}
