using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;

namespace SmartMeter.Model;

// the meter side of the authentication exchange, as a black box:
// the caller asks for a token and gets one; all crypto and wire
// details stay inside this class
public sealed class MeterAuthClient
{
    private readonly Uri _baseUri;
    private readonly HttpClient _http;
    private readonly ECDsa _key;

    public MeterAuthClient(Uri baseUri, HttpClient http, ECDsa key)
    {
        _baseUri = baseUri;
        _http = http;
        _key = key;
    }

    // enrol a new key or reconnect a known one; the secret is only
    // needed the first time, when the server has not seen the key yet
    public async Task<TokenResponse> RequestTokenAsync(
        string? enrolmentSecret,
        CancellationToken cancellationToken = default)
    {
        byte[] publicKey = _key.ExportSubjectPublicKeyInfo();
        ChallengeResponse challenge =
            await RequestChallengeAsync(publicKey, enrolmentSecret, cancellationToken);

        // sign the nonce bytes exactly as they came back
        byte[] nonce = Convert.FromBase64String(challenge.Nonce);
        byte[] signature = _key.SignData(
            nonce,
            HashAlgorithmName.SHA256,
            DSASignatureFormat.IeeeP1363FixedFieldConcatenation);

        return await ProveAsync(challenge.ChallengeId, signature, cancellationToken);
    }

    private async Task<ChallengeResponse> RequestChallengeAsync(
        byte[] publicKey,
        string? enrolmentSecret,
        CancellationToken cancellationToken)
    {
        ChallengeRequest request = new ChallengeRequest
        {
            PublicKey = Convert.ToBase64String(publicKey),
            EnrolmentSecret = enrolmentSecret,
        };

        AuthEnvelope<ChallengeResponse> envelope =
            await PostAsync<ChallengeResponse>("/api/v1/auth/request", request, cancellationToken);

        if (envelope.Response is null)
        {
            throw new IOException("challenge missing");
        }

        return envelope.Response;
    }

    private async Task<TokenResponse> ProveAsync(
        string challengeId,
        byte[] signature,
        CancellationToken cancellationToken)
    {
        ProofRequest request = new ProofRequest
        {
            ChallengeId = challengeId,
            Signature = Convert.ToBase64String(signature),
        };

        AuthEnvelope<TokenResponse> envelope =
            await PostAsync<TokenResponse>("/api/v1/auth/prove", request, cancellationToken);

        if (envelope.Response is null)
        {
            throw new IOException("token missing");
        }

        return envelope.Response;
    }

    // post one model object as json and read the shared server envelope
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

// the shared server envelope: ok/status/response and an error for refusals
public sealed record AuthEnvelope<T>
{
    public required bool Ok { get; init; }

    public required int Status { get; init; }

    public T? Response { get; init; }

    public AuthEnvelopeError? Error { get; init; }
}

// the error half of the server envelope
public sealed record AuthEnvelopeError
{
    public required string Error { get; init; }

    public required string Message { get; init; }
}