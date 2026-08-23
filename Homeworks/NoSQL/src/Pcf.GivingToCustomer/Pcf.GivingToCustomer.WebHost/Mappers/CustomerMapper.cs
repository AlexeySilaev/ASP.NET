using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.WebHost.gRPC.Contracts;
using Pcf.GivingToCustomer.WebHost.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pcf.GivingToCustomer.WebHost.Mappers
{
    public class CustomerMapper
    {

        public static Customer MapFromModel(CreateOrEditCustomerRequest model, IEnumerable<Preference> preferences, Customer customer = null)
        {
            if(customer == null)
            {
                customer = new Customer();
                customer.Id = Guid.NewGuid();
            }
            
            customer.FirstName = model.FirstName;
            customer.LastName = model.LastName;
            customer.Email = model.Email;

            customer.Preferences = preferences.Select(x => new CustomerPreference()
            {
                CustomerId = customer.Id,
                Preference = x,
                PreferenceId = x.Id
            }).ToList();
            
            return customer;
        }

        public static CustomerReply MapFromCustomer(Customer customer)
        {
            CustomerReply result = new CustomerReply
            {
                Id = customer.Id.ToString(),
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email
            };
            result.Preferences.AddRange(customer.Preferences.Select(p => new PreferenceReply
            {
                Id = p.PreferenceId.ToString(),
                Name = p.Preference.Name
            }));
            return result;
        }
    }
}
