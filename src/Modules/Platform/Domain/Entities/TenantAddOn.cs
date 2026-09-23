namespace Platform.Domain.Entities;

/// <summary>Active add-on attached to a tenant subscription.</summary>
public sealed class TenantAddOn
{
    /// <summary>Subscription foreign key.</summary>
    public Guid SubscriptionId { get; private set; }

    /// <summary>Add-on catalog id.</summary>
    public Guid AddOnId { get; private set; }

    /// <summary>When the add-on was activated.</summary>
    public DateTimeOffset ActivatedAt { get; private set; }

    /// <summary>Navigation.</summary>
    public AddOn? AddOn { get; private set; }

    /// <summary>Navigation.</summary>
    public TenantSubscription? Subscription { get; private set; }

#pragma warning disable CS8618
    private TenantAddOn()
    {
    }
#pragma warning restore CS8618

    /// <summary>Links an add-on to a subscription.</summary>
    public TenantAddOn(Guid subscriptionId, Guid addOnId, DateTimeOffset activatedAtUtc)
    {
        SubscriptionId = subscriptionId;
        AddOnId = addOnId;
        ActivatedAt = activatedAtUtc;
    }
}
