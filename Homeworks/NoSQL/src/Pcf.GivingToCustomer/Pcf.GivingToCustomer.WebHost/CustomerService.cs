using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.WebHost.Mappers;
using Pcf.GivingToCustomer.WebHost.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pcf.GivingToCustomer.WebHost
{
    public class CustomerService
    {
        private readonly IRepository<Customer> _customersRepository;
        private readonly IRepository<Preference> _preferenceRepository;


        public CustomerService(IRepository<Customer> customersRepository, IRepository<Preference> preferenceRepository)
        {
            _customersRepository = customersRepository;
            _preferenceRepository = preferenceRepository;
        }

        public Task<IEnumerable<Customer>> GetAllAsync()
        {
            return _customersRepository.GetAllAsync();
        }

        public Task<Customer> GetByIdAsync(Guid id)
        {
            return _customersRepository.GetByIdAsync(id);
        }

        public async Task<Customer> AddAsync(CreateOrEditCustomerRequest request)
        {
            //Получаем предпочтения из бд и сохраняем большой объект
            var preferences = await _preferenceRepository.GetRangeByIdsAsync(request.PreferenceIds);
            Customer customer = CustomerMapper.MapFromModel(request, preferences);
            await _customersRepository.AddAsync(customer);
            return customer;
        }

        public async Task<bool> UpdateAsync(Guid id, CreateOrEditCustomerRequest request)
        {
            var customer = await _customersRepository.GetByIdAsync(id);
            if (customer == null)
                return false;

            var preferences = await _preferenceRepository.GetRangeByIdsAsync(request.PreferenceIds);

            CustomerMapper.MapFromModel(request, preferences, customer);
            await _customersRepository.UpdateAsync(customer);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var customer = await _customersRepository.GetByIdAsync(id);

            if (customer == null)
                return false;

            await _customersRepository.DeleteAsync(customer);
            return true;
        }

    }
}
