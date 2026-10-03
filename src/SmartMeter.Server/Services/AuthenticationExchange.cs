using System.Security.Cryptography;
using System.Text;

using SmartMeter.Model;
using SmartMeter.Server.Storage;

namespace SmartMeter.Server.Services;

// the public key authentication exchange:
// request returns a challenge; proof returns an access token
public sealed class AuthenticationExchange : IAuthenticationExchange
{
    private readonly IEnrolmentStore _enrolments;
    private readonly IChallengeService _challenges;
    private readonly ITokenService _tokens;
    private readonly AuthConfig _config;

    // meter id -> canonical public key while enrolment is still in progress
    private readonly Dictionary<string, byte[]> _pending = new();
    private readonly object _pendingGate = new();

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

        if (pending)
        {
            if (!AcceptsEnrolmentSecret(request.EnrolmentSecret))
            {
                // unknown key without the shared secret; same answer either way
                return RefuseChallenge(403, "enrolment refused");
            }

            lock (_pendingGate)
            {
                _pending[meterId] = publicKey;
            }
        }

        ChallengeResponse challenge =
            await _challenges.CreateAsync(meterId, pending, cancellationToken);

        return new ChallengeOutcome { Challenge = challenge };
    }

    public async Task<ProofOutcome> ProveAsync(ProofRequest request, CancellationToken cancellationToken = default)
    {
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

        byte[]? publicKey = await RegisteredKeyAsync(challenge, cancellationToken);

        if (publicKey is null)
        {
            return RefuseProof(403, "proof refused");
        }

        using ECDsa verifier = ECDsa.Create();
        verifier.ImportSubjectPublicKeyInfo(publicKey, out int _);

        // the exact nonce bytes as returned, nothing else
        bool valid = verifier.VerifyData(
            challenge.Nonce,
            signature,
            HashAlgorithmName.SHA256,
            DSASignatureFormat.IeeeP1363FixedFieldConcatenation);

        if (!valid)
        {
            return RefuseProof(403, "proof refused");
        }

        if (challenge.PendingEnrolment)
        {
            await CompleteEnrolmentAsync(challenge.MeterId, publicKey, cancellationToken);
        }

        TokenResponse token = await _tokens.IssueAsync(challenge.MeterId, cancellationToken);

        return new ProofOutcome { Token = token };
    }

    // the key a challenge was issued for: pending enrolment or a stored record
    private async Task<byte[]?> RegisteredKeyAsync(
        StoredChallenge challenge,
        CancellationToken cancellationToken)
    {
        if (challenge.PendingEnrolment)
        {
            lock (_pendingGate)
            {
                if (!_pending.TryGetValue(challenge.MeterId, out byte[]? publicKey))
                {
                    return null;
                }

                return publicKey;
            }
        }

        EnrolmentRecord? record = await _enrolments.FindAsync(challenge.MeterId, cancellationToken);

        if (record is null)
        {
            return null;
        }

        return AuthCodec.DecodeBase64(record.PublicKey);
    }

    private async Task CompleteEnrolmentAsync(
        string meterId,
        byte[] publicKey,
        CancellationToken cancellationToken)
    {
        lock (_pendingGate)
        {
            _pending.Remove(meterId);
        }

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