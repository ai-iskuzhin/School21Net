using System.Net;
using System.Text;

namespace School21Net.Tests;

/// <summary>
/// Records outgoing requests and returns canned responses from a supplied responder. The token endpoint is
/// answered with a fixed access token so tests can focus on the API calls.
/// </summary>
internal sealed class StubHttpMessageHandler(Func<HttpRequestMessage, (HttpStatusCode Status, string Body)> responder)
    : HttpMessageHandler
{
    public List<HttpRequestMessage> Requests { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);

        var isToken = request.Method == HttpMethod.Post
            && request.RequestUri!.AbsolutePath.Contains("token", StringComparison.OrdinalIgnoreCase);

        var (status, body) = isToken
            ? (HttpStatusCode.OK, "{\"access_token\":\"stub-token\",\"expires_in\":36000}")
            : responder(request);

        return Task.FromResult(new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
            RequestMessage = request
        });
    }
}
