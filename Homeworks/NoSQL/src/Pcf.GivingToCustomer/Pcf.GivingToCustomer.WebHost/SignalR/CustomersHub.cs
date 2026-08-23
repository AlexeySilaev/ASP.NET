using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.WebHost.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pcf.GivingToCustomer.WebHost.SignalR
{
    public class CustomersHub : Hub
    {
        private readonly CustomerService _customerService;

        public CustomersHub(CustomerService customerService)
        { 
            _customerService = customerService;
        }

        public async Task<IReadOnlyCollection<CustomerShortResponse>> GetCustomersAsync()
        {
            var customers = await _customerService.GetAllAsync();

            return customers.Select(x => new CustomerShortResponse()
            {
                Id = x.Id,
                Email = x.Email,
                FirstName = x.FirstName,
                LastName = x.LastName
            }).ToList();
        }

        public async Task<CustomerResponse> GetCustomerAsync(Guid id)
        {
            var result = await _customerService.GetByIdAsync(id);
            return new CustomerResponse(result);
        }

        public async Task<CustomerResponse> CreateCustomerAsync(CreateOrEditCustomerRequest request)
        {
            Customer result = await _customerService.AddAsync(request);
            return new CustomerResponse(result);
        }

        public async Task<bool> EditCustomersAsync(Guid id, CreateOrEditCustomerRequest request)
        {
            return await _customerService.UpdateAsync(id, request);
        }

        public async Task<bool> DeleteCustomerAsync(Guid id)
        {
            return await _customerService.DeleteAsync(id);
        }
    }
}
