using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models.Customers;

namespace PromoCodeFactory.WebHost.Mapping;

internal static class CustomersMapper
{
    public static Customer ToCustomer(CustomerCreateRequest request, ICollection<Preference> preferences)
    {
        return new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Preferences = preferences
        };
    }

    public static CustomerResponse ToCustomerResponse(Customer customer, IReadOnlyCollection<PromoCode> promoCodes)
    {
        var preferences = customer.Preferences
            .Select(p => PreferencesMapper.ToPreferenceShortResponse(p))
            .ToList()
            .AsReadOnly();
        var promocodeResponces = promoCodes
            .SelectMany(c => CustomerPromoCodesMapper.ToCustomerPromoCodeResponses(c))
            .ToList()
            .AsReadOnly();
        return new CustomerResponse(customer.Id, customer.FirstName, customer.LastName, customer.Email, preferences, promocodeResponces);
    }

    public static CustomerShortResponse ToCustomerShortResponse(Customer customer)
    {
        var preferences = customer.Preferences
            .Select(p => PreferencesMapper.ToPreferenceShortResponse(p))
            .ToList()
            .AsReadOnly();
        return new CustomerShortResponse(customer.Id, customer.FirstName, customer.LastName, customer.Email, preferences);
    }
}
