using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models.PromoCodes;

namespace PromoCodeFactory.WebHost.Mapping;

public static class PromoCodesMapper
{
    public static PromoCodeShortResponse ToPromoCodeShortResponse(PromoCode promoCode)
    {
        return new PromoCodeShortResponse(
            promoCode.Id,
            promoCode.Code,
            promoCode.ServiceInfo,
            promoCode.PartnerName,
            promoCode.BeginDate,
            promoCode.EndDate,
            promoCode.PartnerManager.Id,
            promoCode.Preference.Id);
    }

    public static PromoCode ToPromoCode(PromoCodeCreateRequest request, Employee partnerManager, Preference preference)
    {
        Guid promocodeId = Guid.NewGuid();
        DateTimeOffset createdAt = DateTimeOffset.UtcNow;
        return new PromoCode
        {
            Id = promocodeId,
            Code = request.Code,
            ServiceInfo = request.ServiceInfo,
            PartnerName = request.PartnerName,
            BeginDate = request.BeginDate,
            EndDate = request.EndDate,
            PartnerManager = partnerManager,
            Preference = preference,
            CustomerPromoCodes = preference.Customers.Select(c => new CustomerPromoCode
            {
                Id = Guid.NewGuid(),
                CustomerId = c.Id,
                PromoCodeId = promocodeId,
                CreatedAt = createdAt,
                AppliedAt = null
            }).ToList()
        };
    }

}
