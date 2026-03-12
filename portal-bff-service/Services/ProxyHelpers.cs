using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace PortalBffService.Services;

public static class ProxyHelpers
{
    public static HttpRequestMessage CreateProxyRequest(HttpRequest request, string url, HttpMethod method)
    {
        var proxyRequest = new HttpRequestMessage(method, url);
        if (request.Headers.TryGetValue("Authorization", out var token))
        {
            proxyRequest.Headers.Authorization = AuthenticationHeaderValue.Parse(token.ToString());
        }

        if (request.ContentLength is > 0)
        {
            proxyRequest.Content = new StreamContent(request.Body);
            if (!string.IsNullOrWhiteSpace(request.ContentType))
            {
                proxyRequest.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(request.ContentType);
            }
        }

        return proxyRequest;
    }

    public static async Task<IResult> SendProxy(IHttpClientFactory factory, HttpRequestMessage proxyRequest)
    {
        var client = factory.CreateClient();
        var response = await client.SendAsync(proxyRequest, HttpCompletionOption.ResponseHeadersRead);
        var body = await response.Content.ReadAsStringAsync();
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";
        return Results.Content(body, contentType, Encoding.UTF8, (int)response.StatusCode);
    }
}
