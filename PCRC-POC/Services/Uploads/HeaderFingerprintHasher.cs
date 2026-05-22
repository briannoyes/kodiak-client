using System.Security.Cryptography;
using System.Text;

namespace PCRC.Services.Uploads;

/// Header fingerprint = SHA256(sorted, lower-cased, joined with '\n') as a 64-char lowercase hex
/// string. Matches the spec in KodiakMultiSelectPaymentRecordUploadSequence.puml and the column
/// definition <c>HeaderFingerprint char(64)</c> in PaymentMappingTemplates / Documents.
internal static class HeaderFingerprintHasher
{
    public static string Compute(IReadOnlyList<string> headers)
    {
        var normalized = headers
            .Select(h => (h ?? string.Empty).Trim().ToLowerInvariant())
            .OrderBy(h => h, StringComparer.Ordinal);

        var joined = string.Join('\n', normalized);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(joined));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
