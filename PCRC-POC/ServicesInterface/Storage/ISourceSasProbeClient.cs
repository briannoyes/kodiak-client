namespace PCRC.ServicesInterface.Storage;

/// Used by the bulk-ingest flow to validate that a caller-supplied container SAS URL is reachable
/// and grants at least list/read permissions before we accept the upload.
public interface ISourceSasProbeClient
{
    Task<bool> CanReadContainerAsync(string sasUrl, CancellationToken cancellationToken);
}
