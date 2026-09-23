using Core.Domain;
using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Persistence;

/// <summary>
/// Global platform catalog (schema <c>dbo</c> only — not tenant-scoped).
/// </summary>
public sealed class PlatformDbContext : DbContext, IPlatformUnitOfWork, IChangeTrackingUnitOfWork
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

    /// <summary>Tenant legal entities (CNPJ).</summary>
    public DbSet<Branch> Branches => Set<Branch>();

    /// <summary>SaaS base plans.</summary>
    public DbSet<Plan> Plans => Set<Plan>();

    /// <summary>Plan module inclusions.</summary>
    public DbSet<PlanIncludedModule> PlanIncludedModules => Set<PlanIncludedModule>();

    /// <summary>SaaS add-on products.</summary>
    public DbSet<AddOn> AddOns => Set<AddOn>();

    /// <summary>Tenant subscriptions.</summary>
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();

    /// <summary>Active tenant add-ons.</summary>
    public DbSet<TenantAddOn> TenantAddOns => Set<TenantAddOn>();

    /// <summary>Per-tenant module overrides.</summary>
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();

    /// <summary>Pending proration rows.</summary>
    public DbSet<SubscriptionAdjustment> SubscriptionAdjustments => Set<SubscriptionAdjustment>();

    /// <summary>Tenant billing customers (9.4).</summary>
    public DbSet<BillingCustomer> BillingCustomers => Set<BillingCustomer>();

    /// <summary>Tenant billing payment methods (9.4).</summary>
    public DbSet<BillingPaymentMethod> BillingPaymentMethods => Set<BillingPaymentMethod>();

    /// <summary>Platform SaaS invoices (9.4).</summary>
    public DbSet<BillingInvoice> BillingInvoices => Set<BillingInvoice>();

    /// <summary>Gateway charges (9.4).</summary>
    public DbSet<BillingCharge> BillingCharges => Set<BillingCharge>();

    /// <summary>Webhook idempotency ledger (9.4).</summary>
    public DbSet<BillingWebhookReceipt> BillingWebhookReceipts => Set<BillingWebhookReceipt>();

    /// <summary>Dunning notices (9.5).</summary>
    public DbSet<DunningNotice> DunningNotices => Set<DunningNotice>();

    /// <summary>Promotional coupons (9.5).</summary>
    public DbSet<Coupon> Coupons => Set<Coupon>();

    /// <summary>Coupon redemptions (9.5).</summary>
    public DbSet<CouponRedemption> CouponRedemptions => Set<CouponRedemption>();

    /// <summary>VetNexus NFS-e rows (9.6).</summary>
    public DbSet<SaasServiceInvoice> SaasServiceInvoices => Set<SaasServiceInvoice>();

    /// <summary>Impersonation sessions (9.6).</summary>
    public DbSet<ImpersonationSession> ImpersonationSessions => Set<ImpersonationSession>();

    /// <summary>Impersonation audit trail (9.6).</summary>
    public DbSet<ImpersonationAuditEntry> ImpersonationAuditEntries => Set<ImpersonationAuditEntry>();

    /// <summary>Staff login attempts across tenants (9.7).</summary>
    public DbSet<PlatformLoginLog> PlatformLoginLogs => Set<PlatformLoginLog>();

    /// <summary>Super Admin configuration change audit (9.7).</summary>
    public DbSet<PlatformChangeAuditEntry> PlatformChangeAuditEntries => Set<PlatformChangeAuditEntry>();

    /// <summary>Partner API keys (9.7).</summary>
    public DbSet<PartnerApiKey> PartnerApiKeys => Set<PartnerApiKey>();

    /// <summary>Daily API request counters per tenant (9.7).</summary>
    public DbSet<TenantRequestDaily> TenantRequestDailies => Set<TenantRequestDaily>();

    /// <summary>Creates the platform catalog context.</summary>
    public PlatformDbContext(DbContextOptions<PlatformDbContext> options)
        : base(options)
    {
    }

    /// <inheritdoc />
    public bool HasPendingChanges() => ChangeTracker.HasChanges();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlatformDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
