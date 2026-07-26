using System.Net;
using System.Text;

namespace School21Net.Tests;

/// <summary>
/// Records outgoing requests (and their form bodies) and returns canned responses from a supplied responder.
/// Auth is external to the client now, so there is no special token handling here — auth-client tests drive
/// the token endpoint through the same responder.
/// </summary>
internal sealed class StubHttpMessageHandler(Func<HttpRequestMessage, (HttpStatusCode Status, string Body)> responder)
    : HttpMessageHandler
{
    public List<HttpRequestMessage> Requests { get; } = [];
    public List<string?> RequestBodies { get; } = [];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        RequestBodies.Add(request.Content is null
            ? null
            : await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));

        var (status, body) = responder(request);
        return new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
            RequestMessage = request
        };
    }
}
