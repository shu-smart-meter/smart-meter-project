using System.Net.Http;
using System.Net.Http.Json;

namespace SmartMeter.Model;

public sealed class MeterAuthClient
{
    private readonly Uri _baseUri;
    private readonly HttpClient _http;

    public MeterAuthClient(Uri baseUri, HttpClient http)
    {
        _baseUri = baseUri;
        _http = http;
    }

    public async Task<TokenResponse> RequestTokenAsync(
        string meterId,
        string clientSecret,
        CancellationToken cancellationToken = default)
    {
        MeterTokenRequest request = new MeterTokenRequest
        {
            MeterId = meterId,
            ClientSecret = clientSecret,
        };

        AuthEnvelope<TokenResponse> envelope =
            await PostAsync<TokenResponse>("/api/v1/auth/token", request, cancellationToken);

        if (envelope.Response is null)
        {
            throw new IOException("token missing");
        }

        return envelope.Response;
    }

    private async Task<AuthEnvelope<T>> PostAsync<T>(
        string path,
        object body,
        CancellationToken cancellationToken)
    {
        Uri url = new Uri(_baseUri, path);

        using HttpResponseMessage response = await _http.PostAsJsonAsync(url, body, cancellationToken);
        response.EnsureSuccessStatusCode();

        AuthEnvelope<T>? envelope =
            await response.Content.ReadFromJsonAsync<AuthEnvelope<T>>(cancellationToken: cancellationToken);

        if (envelope is null)
        {
            throw new IOException("empty response");
        }

        return envelope;
    }
}

public sealed record AuthEnvelope<T>
{
    public required bool Ok { get; init; }

    public required int Status { get; init; }

    public T? Response { get; init; }

    public string? Error { get; init; }

    public string? Message { get; init; }
}
