using PCRC.Model.Payments;

namespace PCRC.DataInterface.RepositoryInterfaces;

public interface IPaymentRepository : IExternallyIdentifiedRepository<Payment>
{
    Task<List<Payment>> GetByDocumentAsync(long documentId);
    Task<List<Payment>> GetByClientAsync(long clientId);
    Task<List<Payment>> GetByClientAndDateRangeAsync(long clientId, DateOnly start, DateOnly endInclusive);
    Task<List<Payment>> GetByClientAndVendorAsync(long clientId, string vendorId);

    /// INSERT-IF-NOT-EXISTS by the natural unique key (ClientId, Company, CheckNumber). Returns the
    /// number of rows actually inserted — duplicates are silently skipped per the data model.
    Task<int> InsertIfNotExistsAsync(IEnumerable<Payment> payments);
}
