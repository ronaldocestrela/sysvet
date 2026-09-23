using API.IntegrationTests.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace API.IntegrationTests;

/// <summary>
/// Applies EF migrations for module databases used by integration and sync tests.
/// </summary>
internal static class IntegrationTestDatabaseHelper
{
    /// <summary>Matches <c>TenancySettings:SingleTenantId</c> in integration test configuration.</summary>
    internal static readonly Guid SingleTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    /// <summary>
    /// Migrates module schemas used by sync pull/push (Veterinary, Inventory, Sales).
    /// </summary>
    public static async Task ResetModuleDatabasesAsync(IServiceScope scope)
    {
        foreach (var ctx in new DbContext[]
                 {
                     scope.ServiceProvider.GetRequiredService<global::Veterinary.Infrastructure.Persistence.VeterinaryDbContext>(),
                     scope.ServiceProvider.GetRequiredService<global::Inventory.Infrastructure.Persistence.InventoryDbContext>(),
                     scope.ServiceProvider.GetRequiredService<global::Sales.Infrastructure.Persistence.SalesDbContext>(),
                     scope.ServiceProvider.GetRequiredService<global::Petshop.Infrastructure.Persistence.PetshopDbContext>(),
                     scope.ServiceProvider.GetRequiredService<global::Finance.Infrastructure.Persistence.FinanceDbContext>(),
                     scope.ServiceProvider.GetRequiredService<global::Fiscal.Infrastructure.Persistence.FiscalDbContext>(),
                     scope.ServiceProvider.GetRequiredService<global::Automations.Infrastructure.Persistence.AutomationsDbContext>(),
                     scope.ServiceProvider.GetRequiredService<global::TutorPortal.Infrastructure.Persistence.TutorPortalDbContext>(),
                     scope.ServiceProvider.GetRequiredService<global::ClinicSite.Infrastructure.Persistence.ClinicSiteDbContext>(),
                     scope.ServiceProvider.GetRequiredService<global::Commerce.Infrastructure.Persistence.CommerceDbContext>(),
                     scope.ServiceProvider.GetRequiredService<global::Platform.Infrastructure.Persistence.PlatformDbContext>()
                 })
        {
            await ctx.Database.EnsureDeletedAsync();
        }
    }

    public static async Task MigrateModuleDatabasesAsync(IServiceScope scope)
    {
        var vetContext = scope.ServiceProvider.GetRequiredService<global::Veterinary.Infrastructure.Persistence.VeterinaryDbContext>();
        await vetContext.Database.MigrateAsync();
        await EnsureMedicalRecordFollowUpColumnAsync(vetContext);

        var inventoryContext = scope.ServiceProvider.GetRequiredService<global::Inventory.Infrastructure.Persistence.InventoryDbContext>();
        await inventoryContext.Database.MigrateAsync();

        var salesContext = scope.ServiceProvider.GetRequiredService<global::Sales.Infrastructure.Persistence.SalesDbContext>();
        await salesContext.Database.MigrateAsync();

        var petshopContext = scope.ServiceProvider.GetRequiredService<global::Petshop.Infrastructure.Persistence.PetshopDbContext>();
        await petshopContext.Database.MigrateAsync();

        var financeContext = scope.ServiceProvider.GetRequiredService<global::Finance.Infrastructure.Persistence.FinanceDbContext>();
        await financeContext.Database.MigrateAsync();

        var fiscalContext = scope.ServiceProvider.GetRequiredService<global::Fiscal.Infrastructure.Persistence.FiscalDbContext>();
        await fiscalContext.Database.MigrateAsync();

        var automationsContext = scope.ServiceProvider.GetRequiredService<global::Automations.Infrastructure.Persistence.AutomationsDbContext>();
        await automationsContext.Database.MigrateAsync();
        await EnsureAutomations821SchemaAsync(automationsContext);
        await EnsureAutomations83SchemaAsync(automationsContext);

        var tutorPortalContext = scope.ServiceProvider.GetRequiredService<global::TutorPortal.Infrastructure.Persistence.TutorPortalDbContext>();
        await tutorPortalContext.Database.MigrateAsync();

        var clinicSiteContext = scope.ServiceProvider.GetRequiredService<global::ClinicSite.Infrastructure.Persistence.ClinicSiteDbContext>();
        await clinicSiteContext.Database.MigrateAsync();

        var commerceContext = scope.ServiceProvider.GetRequiredService<global::Commerce.Infrastructure.Persistence.CommerceDbContext>();
        await commerceContext.Database.MigrateAsync();

        var platformContext = scope.ServiceProvider.GetRequiredService<global::Platform.Infrastructure.Persistence.PlatformDbContext>();
        await platformContext.Database.MigrateAsync();
        await EnsurePlatformDunningSchemaAsync(platformContext);
        await EnsurePlatformNfseImpersonationSchemaAsync(platformContext);
        await PlatformCatalogTestSeeder.EnsureCatalogAsync(scope.ServiceProvider);

        await EnsureDefaultPlatformTenantAsync(scope, platformContext);
    }

    private static async Task EnsureDefaultPlatformTenantAsync(
        IServiceScope scope,
        global::Platform.Infrastructure.Persistence.PlatformDbContext platformContext)
    {
        if (!await platformContext.Tenants.AnyAsync(t => t.Id == SingleTenantId))
        {
            var tenant = global::Platform.Domain.Entities.Tenant.Create(SingleTenantId, "integration", "Integration Test").Value;
            platformContext.Tenants.Add(tenant);
            await platformContext.SaveChangesAsync();
        }

        var provisioner = scope.ServiceProvider.GetRequiredService<global::Platform.Application.Provisioning.ITenantSubscriptionProvisioner>();
        await provisioner.ProvisionGrandfatherAsync(SingleTenantId);
    }

    private static async Task EnsureAutomations821SchemaAsync(global::Automations.Infrastructure.Persistence.AutomationsDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS AutomationsSettings (
                Id TEXT NOT NULL PRIMARY KEY,
                Key TEXT NOT NULL,
                TimeZoneId TEXT NOT NULL,
                BusinessStart TEXT NOT NULL,
                BusinessEnd TEXT NOT NULL,
                BusinessDaysJson TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                RowVersion BLOB NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS IX_AutomationsSettings_Key ON AutomationsSettings (Key);
            CREATE TABLE IF NOT EXISTS TutorMessagingPreferences (
                Id TEXT NOT NULL PRIMARY KEY,
                TutorId TEXT NOT NULL,
                WhatsAppEnabled INTEGER NOT NULL,
                EmailEnabled INTEGER NOT NULL,
                UpdatedAt TEXT NOT NULL,
                RowVersion BLOB NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS IX_TutorMessagingPreferences_TutorId ON TutorMessagingPreferences (TutorId);
            """);
    }

    private static async Task EnsureAutomations83SchemaAsync(global::Automations.Infrastructure.Persistence.AutomationsDbContext context)
    {
        try
        {
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE TutorMessagingPreferences ADD COLUMN MarketingEnabled INTEGER NOT NULL DEFAULT 1;");
        }
        catch
        {
            // Column already exists.
        }

        await context.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS Campaigns (
                Id TEXT NOT NULL PRIMARY KEY,
                Name TEXT NOT NULL,
                SegmentKind TEXT NOT NULL,
                Status TEXT NOT NULL,
                TemplateCode TEXT NOT NULL,
                InactiveDays INTEGER NOT NULL,
                CooldownDays INTEGER NOT NULL,
                UpdatedAt TEXT NOT NULL,
                RowVersion BLOB NOT NULL
            );
            CREATE TABLE IF NOT EXISTS CampaignRuns (
                Id TEXT NOT NULL PRIMARY KEY,
                CampaignId TEXT NOT NULL,
                StartedAt TEXT NOT NULL,
                AudienceCount INTEGER NOT NULL,
                EnqueuedCount INTEGER NOT NULL,
                UpdatedAt TEXT NOT NULL,
                RowVersion BLOB NOT NULL,
                FOREIGN KEY (CampaignId) REFERENCES Campaigns (Id) ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS IX_CampaignRuns_CampaignId ON CampaignRuns (CampaignId);
            CREATE TABLE IF NOT EXISTS NpsInvites (
                Id TEXT NOT NULL PRIMARY KEY,
                TutorId TEXT NOT NULL,
                TokenHash TEXT NOT NULL,
                ExpiresAt TEXT NOT NULL,
                Status TEXT NOT NULL,
                Score INTEGER NULL,
                Comment TEXT NULL,
                RespondedAt TEXT NULL,
                SourceType TEXT NOT NULL,
                SourceId TEXT NOT NULL,
                CampaignId TEXT NULL,
                UpdatedAt TEXT NOT NULL,
                RowVersion BLOB NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS IX_NpsInvites_SourceType_SourceId ON NpsInvites (SourceType, SourceId);
            CREATE INDEX IF NOT EXISTS IX_NpsInvites_TutorId ON NpsInvites (TutorId);
            """);
    }

    private static async Task EnsurePlatformDunningSchemaAsync(global::Platform.Infrastructure.Persistence.PlatformDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS PlatformCoupons (
                Id TEXT NOT NULL PRIMARY KEY,
                Code TEXT NOT NULL,
                DiscountType INTEGER NOT NULL,
                Value TEXT NOT NULL,
                MaxRedemptions INTEGER NULL,
                RedemptionCount INTEGER NOT NULL,
                ExpiresAt TEXT NULL,
                IsActive INTEGER NOT NULL,
                UpdatedAt TEXT NOT NULL,
                RowVersion BLOB NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS IX_PlatformCoupons_Code ON PlatformCoupons (Code);
            CREATE TABLE IF NOT EXISTS PlatformCouponRedemptions (
                Id TEXT NOT NULL PRIMARY KEY,
                CouponId TEXT NOT NULL,
                TenantId TEXT NOT NULL,
                BillingInvoiceId TEXT NULL,
                RedeemedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                RowVersion BLOB NOT NULL,
                FOREIGN KEY (CouponId) REFERENCES PlatformCoupons (Id) ON DELETE CASCADE
            );
            CREATE UNIQUE INDEX IF NOT EXISTS IX_PlatformCouponRedemptions_TenantId_CouponId
                ON PlatformCouponRedemptions (TenantId, CouponId);
            CREATE TABLE IF NOT EXISTS PlatformDunningNotices (
                Id TEXT NOT NULL PRIMARY KEY,
                TenantId TEXT NOT NULL,
                InvoiceId TEXT NOT NULL,
                StepDay INTEGER NOT NULL,
                Channel INTEGER NOT NULL,
                ScheduledAt TEXT NOT NULL,
                SentAt TEXT NULL,
                UpdatedAt TEXT NOT NULL,
                RowVersion BLOB NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS IX_PlatformDunningNotices_TenantId_InvoiceId_StepDay_Channel
                ON PlatformDunningNotices (TenantId, InvoiceId, StepDay, Channel);
            """);

        foreach (var alter in new[]
                 {
                     "ALTER TABLE \"PlatformBillingInvoices\" ADD COLUMN \"CardRetryCount\" INTEGER NOT NULL DEFAULT 0;",
                     "ALTER TABLE \"PlatformBillingInvoices\" ADD COLUMN \"NextCardRetryAt\" TEXT NULL;",
                     "ALTER TABLE \"PlatformBillingInvoices\" ADD COLUMN \"AppliedCouponId\" TEXT NULL;",
                     "ALTER TABLE \"PlatformTenantSubscriptions\" ADD COLUMN \"PastDueSince\" TEXT NULL;",
                     "ALTER TABLE \"PlatformTenantSubscriptions\" ADD COLUMN \"PendingCouponId\" TEXT NULL;"
                 })
        {
            try
            {
                await context.Database.ExecuteSqlRawAsync(alter);
            }
            catch
            {
                // Column already exists.
            }
        }
    }

    private static async Task EnsurePlatformNfseImpersonationSchemaAsync(global::Platform.Infrastructure.Persistence.PlatformDbContext context)
    {
        foreach (var alter in new[]
                 {
                     "ALTER TABLE \"PlatformBranches\" ADD COLUMN \"PostalCode\" TEXT NULL;",
                     "ALTER TABLE \"PlatformBranches\" ADD COLUMN \"Street\" TEXT NULL;",
                     "ALTER TABLE \"PlatformBranches\" ADD COLUMN \"StreetNumber\" TEXT NULL;",
                     "ALTER TABLE \"PlatformBranches\" ADD COLUMN \"District\" TEXT NULL;",
                     "ALTER TABLE \"PlatformBranches\" ADD COLUMN \"City\" TEXT NULL;",
                     "ALTER TABLE \"PlatformBranches\" ADD COLUMN \"StateCode\" TEXT NULL;",
                     "ALTER TABLE \"PlatformBranches\" ADD COLUMN \"IbgeCode\" INTEGER NULL;"
                 })
        {
            try
            {
                await context.Database.ExecuteSqlRawAsync(alter);
            }
            catch
            {
                // Column already exists.
            }
        }

        await context.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS PlatformSaasServiceInvoices (
                Id TEXT NOT NULL PRIMARY KEY,
                BillingInvoiceId TEXT NOT NULL,
                TenantId TEXT NOT NULL,
                Amount TEXT NOT NULL,
                RecipientCnpj TEXT NOT NULL,
                RecipientLegalName TEXT NOT NULL,
                Status INTEGER NOT NULL,
                NfseNumber TEXT NULL,
                AccessKey TEXT NULL,
                XmlBlobKey TEXT NULL,
                FailureReason TEXT NULL,
                UpdatedAt TEXT NOT NULL,
                RowVersion BLOB NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS IX_PlatformSaasServiceInvoices_BillingInvoiceId
                ON PlatformSaasServiceInvoices (BillingInvoiceId);
            CREATE TABLE IF NOT EXISTS PlatformImpersonationSessions (
                Id TEXT NOT NULL PRIMARY KEY,
                ActorUserId TEXT NOT NULL,
                ActorEmail TEXT NOT NULL,
                TargetTenantId TEXT NOT NULL,
                ClientIp TEXT NOT NULL,
                StartedAt TEXT NOT NULL,
                ExpiresAt TEXT NOT NULL,
                EndedAt TEXT NULL,
                UpdatedAt TEXT NOT NULL,
                RowVersion BLOB NOT NULL
            );
            CREATE TABLE IF NOT EXISTS PlatformImpersonationAuditEntries (
                Id TEXT NOT NULL PRIMARY KEY,
                SessionId TEXT NOT NULL,
                ActorUserId TEXT NOT NULL,
                TargetTenantId TEXT NOT NULL,
                Action TEXT NOT NULL,
                OccurredAt TEXT NOT NULL,
                ClientIp TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                RowVersion BLOB NOT NULL
            );
            """);
    }

    private static async Task EnsureMedicalRecordFollowUpColumnAsync(global::Veterinary.Infrastructure.Persistence.VeterinaryDbContext context)
    {
        try
        {
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE MedicalRecords ADD COLUMN FollowUpOn TEXT NULL;");
        }
        catch
        {
            // Column already exists.
        }
    }
}
