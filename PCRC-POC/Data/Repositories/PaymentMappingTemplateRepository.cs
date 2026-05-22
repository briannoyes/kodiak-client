using Microsoft.EntityFrameworkCore;
using PCRC.DataInterface;
using PCRC.DataInterface.RepositoryInterfaces;
using PCRC.Model.Payments;

namespace PCRC.Data.Repositories;

public class PaymentMappingTemplateRepository
    : AuditedSaveBaseRepository<PaymentMappingTemplate>, IPaymentMappingTemplateRepository
{
    public PaymentMappingTemplateRepository(PcrcDbContext context, IUserContext userContext)
        : base(context, userContext)
    {
    }

    public async Task<PaymentMappingTemplate?> GetByClientAndFingerprintAsync(long clientId, string headerFingerprint)
        => await _set.SingleOrDefaultAsync(x => x.ClientId == clientId
                                                && x.HeaderFingerprint == headerFingerprint);

    public async Task<List<PaymentMappingTemplate>> GetByClientAsync(long clientId)
        => await _set.Where(x => x.ClientId == clientId)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync();
}
