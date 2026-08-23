using Grpc.Core;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.WebHost.gRPC.Contracts;
using Pcf.GivingToCustomer.WebHost.Mappers;
using Pcf.GivingToCustomer.WebHost.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pcf.GivingToCustomer.WebHost.gRPC
{
    public class CustomerGrpcService : CustomersApi.CustomersApiBase
    {
        private readonly CustomerService _customerService;

        public CustomerGrpcService(CustomerService customerService) 
        {
            _customerService = customerService;
        }

        public override async Task<CustomersReply> GetAll(GetAllCustomersRequest request, ServerCallContext context)
        {
            var result = new CustomersReply();
            result.Customers.AddRange((await _customerService.GetAllAsync())
                .Select(customer => CustomerMapper.MapFromCustomer(customer)));
            return result;
        }

        public override async Task<CustomerReply> GetById(CustomerIdRequest request, ServerCallContext context)
        {
            Guid id = Guid.Parse(request.Id);
            Customer result = await _customerService.GetByIdAsync(id);
            return CustomerMapper.MapFromCustomer(result);
        }

        public override async Task<CustomerReply> Create(CreateCustomerRequest request, ServerCallContext context)
        {
            CreateOrEditCustomerRequest ceo = new CreateOrEditCustomerRequest
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PreferenceIds = request.PreferenceIds.Select(Guid.Parse).ToList()
            };
            Customer result = await _customerService.AddAsync(ceo);
            return CustomerMapper.MapFromCustomer(result);
        }

        public override async Task<BooleanCustomerReply> Update(UpdateCustomerRequest request, ServerCallContext context)
        {
            CreateOrEditCustomerRequest ceo = new CreateOrEditCustomerRequest
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PreferenceIds = request.PreferenceIds.Select(Guid.Parse).ToList()
            };
            bool result = await _customerService.UpdateAsync(Guid.Parse(request.Id), ceo);
            return new BooleanCustomerReply { Success = result };
        }

        public override async Task<BooleanCustomerReply> Delete(CustomerIdRequest request, ServerCallContext context)
        {
            bool result = await _customerService.DeleteAsync(Guid.Parse(request.Id));
            return new BooleanCustomerReply { Success = result };
        }
    }
}
