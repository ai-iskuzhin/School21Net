namespace School21Net.Authentication;

/// <summary>
/// A token set returned by <see cref="School21AuthClient"/>. The SDK does not store credentials or refresh
/// internally — the integrator owns the token lifecycle and decides when to call
/// <see cref="School21AuthClient.RefreshAsync"/> or re-authenticate. Persist <see cref="RefreshToken"/>
/// securely if you want to refresh without re-sending the password.
/// </summary>
public sealed record School21Token
{
    /// <summary>The bearer access token to send to the public API.</summary>
    public required string AccessToken { get; init; }

    /// <summary>The refresh token, if the server issued one. Use it with <see cref="School21AuthClient.RefreshAsync"/>.</summary>
    public string? RefreshToken { get; init; }

    /// <summary>Access-token lifetime in seconds, as reported by the server (<c>expires_in</c>).</summary>
    public int ExpiresIn { get; init; }

    /// <summary>Refresh-token lifetime in seconds, as reported by the server (<c>refresh_expires_in</c>); 0 if unknown.</summary>
    public int RefreshExpiresIn { get; init; }

    /// <summary>Token type reported by the server (typically <c>Bearer</c>).</summary>
    public string? TokenType { get; init; }

    /// <summary>UTC instant the token was obtained. Set by <see cref="School21AuthClient"/> when the response is parsed.</summary>
    public DateTimeOffset ObtainedAtUtc { get; init; }

    /// <summary>Absolute UTC expiry of the access token (<see cref="ObtainedAtUtc"/> + <see cref="ExpiresIn"/>).</summary>
    public DateTimeOffset AccessTokenExpiresAtUtc => ObtainedAtUtc.AddSeconds(ExpiresIn);

    /// <summary>Absolute UTC expiry of the refresh token, or <c>null</c> if the server did not report one.</summary>
    public DateTimeOffset? RefreshTokenExpiresAtUtc =>
        RefreshExpiresIn > 0 ? ObtainedAtUtc.AddSeconds(RefreshExpiresIn) : null;
}
