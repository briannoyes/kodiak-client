using PCRC.Model.Payments;

namespace PCRC.DataInterface.RepositoryInterfaces;

public interface IPaymentMappingTemplateRepository : IExternallyIdentifiedRepository<PaymentMappingTemplate>
{
    Task<PaymentMappingTemplate?> GetByClientAndFingerprintAsync(long clientId, string headerFingerprint);
    Task<List<PaymentMappingTemplate>> GetByClientAsync(long clientId);
}
