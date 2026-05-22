using Microsoft.EntityFrameworkCore;
using PCRC.DataInterface;
using PCRC.DataInterface.RepositoryInterfaces;
using PCRC.Model.Payments;

namespace PCRC.Data.Repositories;

public class PaymentRepository : ExternallyIdentifiedRepository<Payment>, IPaymentRepository
{
    private readonly IUserContext _userContext;

    public PaymentRepository(PcrcDbContext context, IUserContext userContext) : base(context)
    {
        _userContext = userContext;
    }

    public async Task<List<Payment>> GetByDocumentAsync(long documentId)
        => await _set.Where(x => x.DocumentId == documentId)
            .OrderBy(x => x.Id)
            .ToListAsync();

    public async Task<List<Payment>> GetByClientAsync(long clientId)
        => await _set.Where(x => x.ClientId == clientId)
            .OrderByDescending(x => x.CheckDate)
            .ToListAsync();

    public async Task<List<Payment>> GetByClientAndDateRangeAsync(long clientId, DateOnly start, DateOnly endInclusive)
        => await _set.Where(x => x.ClientId == clientId
                                 && x.CheckDate != null
                                 && x.CheckDate >= start
                                 && x.CheckDate <= endInclusive)
            .OrderByDescending(x => x.CheckDate)
            .ToListAsync();

    public async Task<List<Payment>> GetByClientAndVendorAsync(long clientId, string vendorId)
        => await _set.Where(x => x.ClientId == clientId && x.VendorID == vendorId)
            .OrderByDescending(x => x.CheckDate)
            .ToListAsync();

    /// Filters out rows whose natural key (ClientId, Company, CheckNumber) already exists, then bulk-adds
    /// the survivors. The DB-level UNIQUE index is the authoritative dedup; this pre-filter just keeps the
    /// happy path from raising and rolling back. Returns the count of rows actually inserted.
    public async Task<int> InsertIfNotExistsAsync(IEnumerable<Payment> payments)
    {
        var incoming = payments.ToList();
        if (incoming.Count == 0)
        {
            return 0;
        }

        var utcNow = DateTime.UtcNow;
        var userId = _userContext.UserId;

        var clientIds = incoming.Select(p => p.ClientId).Distinct().ToList();
        var checkNumbers = incoming.Select(p => p.CheckNumber).Distinct().ToList();
        var existingKeys = await _set
            .Where(x => clientIds.Contains(x.ClientId) && checkNumbers.Contains(x.CheckNumber))
            .Select(x => new { x.ClientId, x.Company, x.CheckNumber })
            .ToListAsync();
        var existing = new HashSet<(long, string?, string?)>(
            existingKeys.Select(k => (k.ClientId, k.Company, k.CheckNumber)));

        var toInsert = incoming
            .Where(p => existing.Add((p.ClientId, p.Company, p.CheckNumber)))
            .ToList();

        foreach (var payment in toInsert)
        {
            if (payment.CreatedAt == default)
            {
                payment.CreatedAt = utcNow;
            }
            payment.CreatedByUserId ??= userId;
            payment.ProcessedAt ??= utcNow;
        }

        await _set.AddRangeAsync(toInsert);
        return toInsert.Count;
    }
}
