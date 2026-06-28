using MassTransit;
using Pcf.Administration.Core;
using SharedModels;
using System;
using System.Threading.Tasks;

namespace Pcf.Administration.WebHost.MassTransit
{
    public class EmployeePromocodeConsumer : IConsumer<ISupportPartnerManagerId>
    {
        private readonly EmployeePromocodeEngine _employeePromocodeEngine;

        public EmployeePromocodeConsumer(EmployeePromocodeEngine employeePromocodeEngine)
        { 
            _employeePromocodeEngine = employeePromocodeEngine;
        }

        public async Task Consume(ConsumeContext<ISupportPartnerManagerId> context)
        {
            Guid employeeId = context.Message.PartnerManagerId.Value;
            await _employeePromocodeEngine.ApplyAsync(employeeId);
        }
    }
}
