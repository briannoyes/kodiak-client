namespace PCRC.ServicesInterface.Storage;

/// Reads sheet 1, row 1 of the supplied Excel stream and returns the header cell values in the
/// order they appear. Used inline during the payment-record upload flow to compute the header
/// fingerprint without waiting on a worker.
public interface IExcelHeaderReader
{
    Task<IReadOnlyList<string>> ReadFirstRowAsync(Stream content, CancellationToken cancellationToken);
}
