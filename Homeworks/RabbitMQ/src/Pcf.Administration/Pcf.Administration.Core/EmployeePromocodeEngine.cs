using Pcf.Administration.Core.Abstractions.Repositories;
using Pcf.Administration.Core.Domain.Administration;
using System;
using System.Threading.Tasks;

namespace Pcf.Administration.Core
{
    public class EmployeePromocodeEngine
    {
        private readonly IRepository<Employee> _employeeRepository;

        public EmployeePromocodeEngine(IRepository<Employee> employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<bool> ApplyAsync(Guid employeeId)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);

            if (employee == null)
                return false;

            employee.AppliedPromocodesCount++;

            await _employeeRepository.UpdateAsync(employee);

            return true;
        }
    }
}
