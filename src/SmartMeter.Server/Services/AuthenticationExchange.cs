using System.Security.Cryptography;
using System.Text;

using SmartMeter.Model;
using SmartMeter.Server.Storage;

namespace SmartMeter.Server.Services;

// the public key authentication exchange:
// request returns a challenge; proof returns an access token
public sealed class AuthenticationExchange : IAuthenticationExchange
{
    // base64 of a P-256 spki fits in 124 chars; anything bigger is not a key
    private static readonly int MaximumPublicKeyLength = 1024;
    private static readonly int MaximumEnrolmentSecretLength = 512;
    private static readonly int MaximumChallengeIdLength = 128;
    private static readonly int MaximumSignatureLength = 256;

    private readonly IEnrolmentStore _enrolments;
    private readonly IChallengeService _challenges;
    private readonly ITokenService _tokens;
    private readonly AuthConfig _config;

    public AuthenticationExchange(
        IEnrolmentStore enrolments,
        IChallengeService challenges,
        ITokenService tokens,
        AuthConfig config)
    {
        _enrolments = enrolments;
        _challenges = challenges;
        _tokens = tokens;
        _config = config;
    }

    public async Task<ChallengeOutcome> RequestChallengeAsync(
        ChallengeRequest request,
        CancellationToken cancellationToken = default)
    {
        // reject oversized or malformed input before any decode work
        if (request.PublicKey.Length > MaximumPublicKeyLength)
        {
            return RefuseChallenge(400, "request not accepted");
        }

        string? enrolmentSecret = request.EnrolmentSecret;

        if (enrolmentSecret is not null && enrolmentSecret.Length > MaximumEnrolmentSecretLength)
        {
            return RefuseChallenge(400, "request not accepted");
        }

        byte[]? publicKey = AuthCodec.DecodeBase64(request.PublicKey);

        if (publicKey is null)
        {
            return RefuseChallenge(400, "request not accepted");
        }

        // canonical form, so the identity cannot depend on how the key was encoded
        publicKey = AuthCodec.CanonicalP256Key(publicKey);

        if (publicKey is null)
        {
            return RefuseChallenge(400, "request not accepted");
        }

        string meterId = AuthCodec.DeriveMeterId(publicKey);
        EnrolmentRecord? existing = await _enrolments.FindAsync(meterId, cancellationToken);
        bool pending = existing is null;

        if (pending && !AcceptsEnrolmentSecret(enrolmentSecret))
        {
            // unknown key without the shared secret; same answer either way
            return RefuseChallenge(403, "enrolment refused");
        }

        ChallengeResponse? challenge =
            await _challenges.CreateAsync(meterId, publicKey, pending, cancellationToken);

        if (challenge is null)
        {
            return RefuseChallenge(503, "try again later");
        }

        return new ChallengeOutcome { Challenge = challenge };
    }

    public async Task<ProofOutcome> ProveAsync(ProofRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ChallengeId.Length > MaximumChallengeIdLength ||
            request.Signature.Length > MaximumSignatureLength)
        {
            return RefuseProof(403, "proof refused");
        }

        StoredChallenge? challenge = await _challenges.ConsumeAsync(request.ChallengeId, cancellationToken);

        if (challenge is null)
        {
            return RefuseProof(403, "proof refused");
        }

        // P-1363 signatures are fixed at 64 bytes for P-256; reject anything else
        byte[]? signature = AuthCodec.DecodeBase64(request.Signature);

        if (signature is null || signature.Length != 64)
        {
            return RefuseProof(403, "proof refused");
        }

        // malformed key material must refuse the proof, never crash the request
        if (!await VerifyProofAsync(challenge, signature))
        {
            return RefuseProof(403, "proof refused");
        }

        if (challenge.PendingEnrolment)
        {
            await CompleteEnrolmentAsync(challenge.MeterId, challenge.PublicKey, cancellationToken);
        }

        TokenResponse? token = await _tokens.IssueAsync(challenge.MeterId, cancellationToken);

        if (token is null)
        {
            return RefuseProof(503, "try again later");
        }

        return new ProofOutcome { Token = token };
    }

    // the signature covers the exact nonce bytes as returned, nothing else
    private async Task<bool> VerifyProofAsync(StoredChallenge challenge, byte[] signature)
    {
        try
        {
            using ECDsa verifier = ECDsa.Create();
            verifier.ImportSubjectPublicKeyInfo(challenge.PublicKey, out int _);

            return verifier.VerifyData(
                challenge.Nonce,
                signature,
                HashAlgorithmName.SHA256,
                DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
        }
        catch (CryptographicException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    // persisting the same key twice is harmless; the record is identical
    private async Task CompleteEnrolmentAsync(
        string meterId,
        byte[] publicKey,
        CancellationToken cancellationToken)
    {
        EnrolmentRecord record = new EnrolmentRecord
        {
            MeterId = meterId,
            PublicKey = AuthCodec.EncodeBase64(publicKey),
            EnrolledAt = DateTimeOffset.UtcNow,
        };

        await _enrolments.SaveAsync(record, cancellationToken);
    }

    // enrolment needs the one shared secret; unset means every key is refused
    private bool AcceptsEnrolmentSecret(string? presented)
    {
        string? expected = _config.EnrolmentSecret;

        if (expected is null || string.IsNullOrEmpty(presented))
        {
            return false;
        }

        byte[] a = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
        byte[] b = SHA256.HashData(Encoding.UTF8.GetBytes(presented));

        return CryptographicOperations.FixedTimeEquals(a, b);
    }

    private static ChallengeOutcome RefuseChallenge(int status, string reason)
    {
        return new ChallengeOutcome { Error = new AuthError { Status = status, Reason = reason } };
    }

    private static ProofOutcome RefuseProof(int status, string reason)
    {
        return new ProofOutcome { Error = new AuthError { Status = status, Reason = reason } };
    }
}