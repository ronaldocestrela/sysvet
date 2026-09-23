using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Platform.Application.Abstractions;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;
using PlatformErrorCodes = Platform.Domain.ErrorCodes;

namespace Platform.Application.Coupons;

/// <summary>Coupon CQRS handlers (9.5).</summary>
public sealed class CouponCommandHandlers :
    IRequestHandler<CreateCouponCommand, Result<CouponSummaryDto>>,
    IRequestHandler<ListCouponsQuery, Result<IReadOnlyList<CouponSummaryDto>>>,
    IRequestHandler<RedeemTenantCouponCommand, Result>
{
    private readonly ICouponRepository _couponRepository;
    private readonly ICouponRedemptionRepository _redemptionRepository;
    private readonly ITenantSubscriptionRepository _subscriptionRepository;
    private readonly IPlatformUnitOfWork _unitOfWork;

    /// <summary>Creates handlers.</summary>
    public CouponCommandHandlers(
        ICouponRepository couponRepository,
        ICouponRedemptionRepository redemptionRepository,
        ITenantSubscriptionRepository subscriptionRepository,
        IPlatformUnitOfWork unitOfWork)
    {
        _couponRepository = couponRepository;
        _redemptionRepository = redemptionRepository;
        _subscriptionRepository = subscriptionRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<Result<CouponSummaryDto>> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        var created = Coupon.Create(
            request.Code,
            request.DiscountType,
            request.Value,
            request.MaxRedemptions,
            request.ExpiresAt);
        if (created.IsFailure)
        {
            return Result.Failure<CouponSummaryDto>(created.Error);
        }

        var existing = await _couponRepository.GetByCodeAsync(created.Value.Code, cancellationToken);
        if (existing is not null)
        {
            return Result.Failure<CouponSummaryDto>(PlatformErrorCodes.Coupon.InvalidCode);
        }

        await _couponRepository.AddAsync(created.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(Map(created.Value));
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<CouponSummaryDto>>> Handle(
        ListCouponsQuery request,
        CancellationToken cancellationToken)
    {
        var coupons = await _couponRepository.ListAsync(cancellationToken);
        var dtos = coupons.Select(Map).ToList();
        return Result.Success<IReadOnlyList<CouponSummaryDto>>(dtos);
    }

    /// <inheritdoc />
    public async Task<Result> Handle(RedeemTenantCouponCommand request, CancellationToken cancellationToken)
    {
        var normalized = Coupon.NormalizeCode(request.Code);
        var coupon = await _couponRepository.GetByCodeAsync(normalized, cancellationToken);
        if (coupon is null)
        {
            return Result.Failure(PlatformErrorCodes.Coupon.NotFound);
        }

        var subscription = await _subscriptionRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);
        if (subscription is null)
        {
            return Result.Failure(PlatformErrorCodes.Subscription.NotFound);
        }

        var already = await _redemptionRepository.ExistsForTenantAsync(request.TenantId, coupon.Id, cancellationToken);
        if (!coupon.CanRedeem(DateTimeOffset.UtcNow, already))
        {
            return Result.Failure(already ? PlatformErrorCodes.Coupon.AlreadyRedeemed : PlatformErrorCodes.Coupon.NotRedeemable);
        }

        var redemption = CouponRedemption.Create(coupon.Id, request.TenantId, DateTimeOffset.UtcNow);
        if (redemption.IsFailure)
        {
            return Result.Failure(redemption.Error);
        }

        var pending = subscription.SetPendingCoupon(coupon.Id);
        if (pending.IsFailure)
        {
            return pending;
        }

        await _redemptionRepository.AddAsync(redemption.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static CouponSummaryDto Map(Coupon coupon) =>
        new(
            coupon.Id,
            coupon.Code,
            coupon.DiscountType,
            coupon.Value,
            coupon.MaxRedemptions,
            coupon.RedemptionCount,
            coupon.ExpiresAt,
            coupon.IsActive);
}
