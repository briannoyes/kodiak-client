using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCRC.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Create : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BillingEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    ArchivedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    ArchiveLocation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    DocumentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UploadId = table.Column<long>(type: "bigint", nullable: true),
                    ClientId = table.Column<long>(type: "bigint", nullable: false),
                    ClientExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    BlobPath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    OriginalFileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    Md5Hash = table.Column<string>(type: "char(32)", nullable: true),
                    Headers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeaderFingerprint = table.Column<string>(type: "char(64)", nullable: true),
                    MappingTemplateId = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ResultRef = table.Column<string>(type: "char(32)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMappingTemplates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    ClientId = table.Column<long>(type: "bigint", nullable: false),
                    HeaderFingerprint = table.Column<string>(type: "char(64)", nullable: false),
                    Mapping = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMappingTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    DocumentId = table.Column<long>(type: "bigint", nullable: false),
                    ClientId = table.Column<long>(type: "bigint", nullable: false),
                    VendorID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VendorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Company = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    InvoiceDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CheckDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CheckNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InvoiceAmount = table.Column<decimal>(type: "decimal(19,4)", nullable: true),
                    CheckAmount = table.Column<decimal>(type: "decimal(19,4)", nullable: true),
                    VoidDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CheckStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PhysicianVendor = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Uploads",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    ClientId = table.Column<long>(type: "bigint", nullable: false),
                    InitiatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TotalCount = table.Column<int>(type: "int", nullable: true),
                    ProcessedCount = table.Column<int>(type: "int", nullable: false),
                    DedupedCount = table.Column<int>(type: "int", nullable: false),
                    FailedCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uploads", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserClientAccess",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ClientId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    GrantedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    RevokedByUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClientAccess", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    EntraObjectId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_ExternalId",
                table: "Clients",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Status_Name",
                table: "Clients",
                columns: new[] { "Status", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ClientId_UploadedAt",
                table: "Documents",
                columns: new[] { "ClientId", "UploadedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocumentType_ClientId_UploadedAt",
                table: "Documents",
                columns: new[] { "DocumentType", "ClientId", "UploadedAt" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ExternalId",
                table: "Documents",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Md5Hash",
                table: "Documents",
                column: "Md5Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UploadedByUserId_UploadedAt",
                table: "Documents",
                columns: new[] { "UploadedByUserId", "UploadedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UploadId",
                table: "Documents",
                column: "UploadId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMappingTemplates_ClientId_HeaderFingerprint",
                table: "PaymentMappingTemplates",
                columns: new[] { "ClientId", "HeaderFingerprint" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMappingTemplates_ExternalId",
                table: "PaymentMappingTemplates",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ClientId_CheckDate",
                table: "Payments",
                columns: new[] { "ClientId", "CheckDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ClientId_Company_CheckNumber",
                table: "Payments",
                columns: new[] { "ClientId", "Company", "CheckNumber" },
                unique: true,
                filter: "[Company] IS NOT NULL AND [CheckNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ClientId_VendorID",
                table: "Payments",
                columns: new[] { "ClientId", "VendorID" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_DocumentId",
                table: "Payments",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ExternalId",
                table: "Payments",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Uploads_ClientId_CreatedAt",
                table: "Uploads",
                columns: new[] { "ClientId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Uploads_ExternalId",
                table: "Uploads",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Uploads_InitiatedByUserId_CreatedAt",
                table: "Uploads",
                columns: new[] { "InitiatedByUserId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Uploads_Status",
                table: "Uploads",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UserClientAccess_ClientId_UserId",
                table: "UserClientAccess",
                columns: new[] { "ClientId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserClientAccess_ExternalId",
                table: "UserClientAccess",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserClientAccess_UserId_ClientId",
                table: "UserClientAccess",
                columns: new[] { "UserId", "ClientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EntraObjectId",
                table: "Users",
                column: "EntraObjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ExternalId",
                table: "Users",
                column: "ExternalId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "PaymentMappingTemplates");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Uploads");

            migrationBuilder.DropTable(
                name: "UserClientAccess");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
