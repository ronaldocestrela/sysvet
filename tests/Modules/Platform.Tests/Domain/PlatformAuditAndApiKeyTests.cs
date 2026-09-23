using FluentAssertions;
using Platform.Domain;
using Platform.Domain.Entities;
using Platform.Domain.Security;

namespace Platform.Tests.Domain;

public class PlatformAuditAndApiKeyTests
{
    [Fact]
    public void PlatformLoginLog_Create_RejectsEmptyEmail()
    {
        var result = PlatformLoginLog.Create(
            null,
            " ",
            succeeded: true,
            "127.0.0.1",
            "Mozilla/5.0",
            "unknown",
            "unknown",
            DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Audit.InvalidEmail);
    }

    [Fact]
    public void PlatformChangeAuditEntry_Create_RejectsInvalidActor()
    {
        var result = PlatformChangeAuditEntry.Create(
            " ",
            Guid.NewGuid(),
            "PlanChanged",
            "{}",
            "10.0.0.1",
            DateTimeOffset.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Audit.InvalidActor);
    }

    [Fact]
    public void PartnerApiKey_Create_StoresHashNotPlainSecret()
    {
        const string secret = "vn_test_secret_value_1234567890";
        var hash = PartnerApiKeyHasher.HashSecret(secret);
        var created = PartnerApiKey.Create(
            Guid.NewGuid(),
            "Contabilidade X",
            PartnerApiKey.ExtractPrefix(secret),
            hash,
            PartnerApiKeyScopes.HealthRead,
            "super-admin-1",
            DateTimeOffset.UtcNow);

        created.IsSuccess.Should().BeTrue();
        created.Value.SecretHash.Should().NotBe(secret);
        created.Value.SecretHash.Should().Be(hash);
        created.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public void PartnerApiKey_Revoke_MarksInactive()
    {
        const string secret = "vn_revoke_me_abcdefghijklmnopqrst";
        var key = PartnerApiKey.Create(
            Guid.NewGuid(),
            "Partner",
            PartnerApiKey.ExtractPrefix(secret),
            PartnerApiKeyHasher.HashSecret(secret),
            PartnerApiKeyScopes.HealthRead,
            "actor",
            DateTimeOffset.UtcNow).Value;

        var revokedAt = DateTimeOffset.UtcNow;
        key.Revoke(revokedAt).IsSuccess.Should().BeTrue();
        key.IsActive.Should().BeFalse();
        key.RevokedAt.Should().Be(revokedAt);
        key.Revoke(revokedAt.AddMinutes(1)).IsFailure.Should().BeTrue();
    }

    [Fact]
    public void TenantRequestDaily_Increment_IncreasesCount()
    {
        var day = DateTimeOffset.Parse("2026-09-23T15:00:00Z");
        var row = TenantRequestDaily.Create(Guid.NewGuid(), day, 0).Value;
        row.Increment(day).IsSuccess.Should().BeTrue();
        row.RequestCount.Should().Be(1);
    }
}
