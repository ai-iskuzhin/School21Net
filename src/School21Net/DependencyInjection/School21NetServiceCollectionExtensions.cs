using Microsoft.Extensions.DependencyInjection;

namespace School21Net.DependencyInjection;

/// <summary>DI helpers for registering <see cref="School21Client"/> with a pooled <c>HttpClient</c>.</summary>
public static class School21NetServiceCollectionExtensions
{
    /// <summary>
    /// Register <see cref="School21Client"/> as a typed <c>HttpClient</c> using the given <paramref name="options"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddSchool21Net(new School21ClientOptions { Username = cfg["School21Net:Username"], Password = cfg["School21Net:Password"] });
    /// </code>
    /// </example>
    public static IHttpClientBuilder AddSchool21Net(this IServiceCollection services, School21ClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        services.AddSingleton(options);
        return services.AddHttpClient<School21Client>();
    }
}
