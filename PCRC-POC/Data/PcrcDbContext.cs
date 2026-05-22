using Microsoft.EntityFrameworkCore;
using PCRC.Model.Authorization;
using PCRC.Model.Clients;
using PCRC.Model.Documents;
using PCRC.Model.Payments;
using PCRC.Model.Uploads;
using PCRC.Model.Users;

namespace PCRC.Data;

public class PcrcDbContext : DbContext
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserClientAccess> UserClientAccess => Set<UserClientAccess>();
    public DbSet<Upload> Uploads => Set<Upload>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentMappingTemplate> PaymentMappingTemplates => Set<PaymentMappingTemplate>();

    public PcrcDbContext(DbContextOptions<PcrcDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureClients(modelBuilder);
        ConfigureUsers(modelBuilder);
        ConfigureUserClientAccess(modelBuilder);
        ConfigureUploads(modelBuilder);
        ConfigureDocuments(modelBuilder);
        ConfigurePayments(modelBuilder);
        ConfigurePaymentMappingTemplates(modelBuilder);
    }

    private static void ConfigureClients(ModelBuilder mb)
    {
        var e = mb.Entity<Client>();
        e.ToTable("Clients");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).UseIdentityColumn();
        e.Property(x => x.ExternalId).HasDefaultValueSql("NEWID()");
        e.HasIndex(x => x.ExternalId).IsUnique();
        e.Property(x => x.Name).HasMaxLength(200).IsRequired();
        e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        e.Property(x => x.BillingEmail).HasMaxLength(320);
        e.Property(x => x.CreatedAt).HasColumnType("datetime2(3)").IsRequired();
        e.Property(x => x.UpdatedAt).HasColumnType("datetime2(3)").IsRequired();
        e.Property(x => x.ArchivedAt).HasColumnType("datetime2(3)");
        e.Property(x => x.ArchiveLocation).HasMaxLength(500);
        e.HasIndex(x => new { x.Status, x.Name });
    }

    private static void ConfigureUsers(ModelBuilder mb)
    {
        var e = mb.Entity<User>();
        e.ToTable("Users");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).UseIdentityColumn();
        e.Property(x => x.ExternalId).HasDefaultValueSql("NEWID()");
        e.HasIndex(x => x.ExternalId).IsUnique();
        e.Property(x => x.EntraObjectId).HasMaxLength(64).IsRequired();
        e.HasIndex(x => x.EntraObjectId).IsUnique();
        e.Property(x => x.Email).HasMaxLength(320).IsRequired();
        e.HasIndex(x => x.Email).IsUnique();
        e.Property(x => x.DisplayName).HasMaxLength(200);
        e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        e.Property(x => x.CreatedAt).HasColumnType("datetime2(3)").IsRequired();
        e.Property(x => x.LastSeenAt).HasColumnType("datetime2(3)");
        e.Property(x => x.DeletedAt).HasColumnType("datetime2(3)");
    }

    private static void ConfigureUserClientAccess(ModelBuilder mb)
    {
        var e = mb.Entity<UserClientAccess>();
        e.ToTable("UserClientAccess");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).UseIdentityColumn();
        e.Property(x => x.ExternalId).HasDefaultValueSql("NEWID()");
        e.HasIndex(x => x.ExternalId).IsUnique();
        e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        e.Property(x => x.GrantedAt).HasColumnType("datetime2(3)").IsRequired();
        e.Property(x => x.RevokedAt).HasColumnType("datetime2(3)");
        e.HasIndex(x => new { x.UserId, x.ClientId }).IsUnique();
        e.HasIndex(x => new { x.ClientId, x.UserId });
    }

    private static void ConfigureUploads(ModelBuilder mb)
    {
        var e = mb.Entity<Upload>();
        e.ToTable("Uploads");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).UseIdentityColumn();
        e.Property(x => x.ExternalId).HasDefaultValueSql("NEWID()");
        e.HasIndex(x => x.ExternalId).IsUnique();
        e.Property(x => x.SourceType).HasConversion<string>().HasMaxLength(10).IsRequired();
        e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        e.Property(x => x.CreatedAt).HasColumnType("datetime2(3)").IsRequired();
        e.Property(x => x.CompletedAt).HasColumnType("datetime2(3)");
        e.HasIndex(x => new { x.ClientId, x.CreatedAt }).IsDescending(false, true);
        e.HasIndex(x => new { x.InitiatedByUserId, x.CreatedAt }).IsDescending(false, true);
        e.HasIndex(x => x.Status);
    }

    private static void ConfigureDocuments(ModelBuilder mb)
    {
        var e = mb.Entity<Document>();
        e.ToTable("Documents");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).UseIdentityColumn();
        e.Property(x => x.ExternalId).HasDefaultValueSql("NEWID()");
        e.HasIndex(x => x.ExternalId).IsUnique();
        e.Property(x => x.DocumentType).HasConversion<string>().HasMaxLength(20).IsRequired();
        e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        e.Property(x => x.BlobPath).HasMaxLength(1024);
        e.Property(x => x.OriginalFileName).HasMaxLength(500);
        e.Property(x => x.ContentType).HasMaxLength(100);
        e.Property(x => x.Md5Hash).HasColumnType("char(32)");
        e.Property(x => x.HeaderFingerprint).HasColumnType("char(64)");
        e.Property(x => x.ResultRef).HasColumnType("char(32)");
        e.Property(x => x.ErrorMessage).HasMaxLength(2000);
        e.Property(x => x.UploadedAt).HasColumnType("datetime2(3)").IsRequired();
        e.Property(x => x.ProcessedAt).HasColumnType("datetime2(3)");
        e.HasIndex(x => new { x.ClientId, x.UploadedAt }).IsDescending(false, true);
        e.HasIndex(x => new { x.UploadedByUserId, x.UploadedAt }).IsDescending(false, true);
        e.HasIndex(x => x.UploadId);
        e.HasIndex(x => x.Md5Hash);
        e.HasIndex(x => new { x.DocumentType, x.ClientId, x.UploadedAt }).IsDescending(false, false, true);
    }

    private static void ConfigurePayments(ModelBuilder mb)
    {
        var e = mb.Entity<Payment>();
        e.ToTable("Payments");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).UseIdentityColumn();
        e.Property(x => x.ExternalId).HasDefaultValueSql("NEWID()");
        e.HasIndex(x => x.ExternalId).IsUnique();
        e.Property(x => x.VendorID).HasMaxLength(50);
        e.Property(x => x.VendorName).HasMaxLength(200);
        e.Property(x => x.Company).HasMaxLength(200);
        e.Property(x => x.CheckNumber).HasMaxLength(50);
        e.Property(x => x.InvoiceAmount).HasColumnType("decimal(19,4)");
        e.Property(x => x.CheckAmount).HasColumnType("decimal(19,4)");
        e.Property(x => x.CheckStatus).HasMaxLength(50);
        e.Property(x => x.CreatedAt).HasColumnType("datetime2(3)").IsRequired();
        e.Property(x => x.ProcessedAt).HasColumnType("datetime2(3)");
        e.HasIndex(x => x.DocumentId);
        e.HasIndex(x => new { x.ClientId, x.Company, x.CheckNumber }).IsUnique();
        e.HasIndex(x => new { x.ClientId, x.CheckDate }).IsDescending(false, true);
        e.HasIndex(x => new { x.ClientId, x.VendorID });
    }

    private static void ConfigurePaymentMappingTemplates(ModelBuilder mb)
    {
        var e = mb.Entity<PaymentMappingTemplate>();
        e.ToTable("PaymentMappingTemplates");
        e.HasKey(x => x.Id);
        e.Property(x => x.Id).UseIdentityColumn();
        e.Property(x => x.ExternalId).HasDefaultValueSql("NEWID()");
        e.HasIndex(x => x.ExternalId).IsUnique();
        e.Property(x => x.HeaderFingerprint).HasColumnType("char(64)").IsRequired();
        e.Property(x => x.Mapping).IsRequired();
        e.Property(x => x.CreatedAt).HasColumnType("datetime2(3)").IsRequired();
        e.Property(x => x.UpdatedAt).HasColumnType("datetime2(3)").IsRequired();
        e.HasIndex(x => new { x.ClientId, x.HeaderFingerprint }).IsUnique();
    }
}
