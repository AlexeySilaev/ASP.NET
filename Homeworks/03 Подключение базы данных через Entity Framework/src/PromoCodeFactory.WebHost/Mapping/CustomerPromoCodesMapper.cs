using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models.PromoCodes;

namespace PromoCodeFactory.WebHost.Mapping;

public static class CustomerPromoCodesMapper
{
    public static IEnumerable<CustomerPromoCodeResponse> ToCustomerPromoCodeResponses(PromoCode promoCode)
    {
        return promoCode.CustomerPromoCodes.Select(cpc => new CustomerPromoCodeResponse(promoCode.Id, promoCode.Code, promoCode.ServiceInfo,
            promoCode.PartnerName, promoCode.BeginDate, promoCode.EndDate, promoCode.PartnerManager.Id, promoCode.Preference.Id,
            cpc.CreatedAt, cpc.AppliedAt));
    }
}
