using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;

namespace SmartMeter.Server.Services;

// wire encodings for the authentication exchange; standard .NET classes only
public sealed class AuthCodec
{
    // base64url without padding, so tokens are safe in headers and urls
    public static string EncodeBase64Url(byte[] bytes)
    {
        return WebEncoders.Base64UrlEncode(bytes);
    }

    public static string EncodeBase64(byte[] bytes)
    {
        return Convert.ToBase64String(bytes);
    }

    public static byte[]? DecodeBase64(string? encoded)
    {
        if (string.IsNullOrWhiteSpace(encoded))
        {
            return null;
        }

        try
        {
            return Convert.FromBase64String(encoded.Trim());
        }
        catch (FormatException)
        {
            return null;
        }
    }

    // the meter id is the hash of the public key, so it is stable across restarts
    public static string DeriveMeterId(byte[] publicKey)
    {
        return Convert.ToHexString(SHA256.HashData(publicKey)).ToLowerInvariant();
    }

    // validate a presented public key and return its canonical form:
    // the whole buffer must parse as one SPKI and the curve must be P-256
    public static byte[]? CanonicalP256Key(byte[] publicKey)
    {
        try
        {
            using ECDsa key = ECDsa.Create();
            key.ImportSubjectPublicKeyInfo(publicKey, out int bytesRead);

            if (bytesRead != publicKey.Length)
            {
                return null;
            }

            ECParameters parameters = key.ExportParameters(false);
            ECCurve expected = ECCurve.NamedCurves.nistP256;

            if (parameters.Curve.Oid.Value != expected.Oid.Value)
            {
                return null;
            }

            return key.ExportSubjectPublicKeyInfo();
        }
        catch (CryptographicException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}