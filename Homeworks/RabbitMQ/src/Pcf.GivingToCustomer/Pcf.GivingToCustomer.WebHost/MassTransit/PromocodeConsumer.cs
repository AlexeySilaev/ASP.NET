using MassTransit;
using Pcf.GivingToCustomer.Core;
using Pcf.GivingToCustomer.WebHost.Mappers;
using Pcf.GivingToCustomer.WebHost.Models;
using SharedModels;
using System.Threading.Tasks;

namespace Pcf.GivingToCustomer.WebHost.MassTransit
{
    public class PromocodeConsumer : IConsumer<IGivePromoCodeToCustomerDto>
    {
        private readonly PromoCodeEngine _promoCodeEngine;

        public PromocodeConsumer(PromoCodeEngine promoCodeEngine)
        {
            _promoCodeEngine = promoCodeEngine;
        }

        public async Task Consume(ConsumeContext<IGivePromoCodeToCustomerDto> context)
        {
            IGivePromoCodeToCustomerDto request = context.Message;
            bool result = await _promoCodeEngine.ApplyAsync(request.PreferenceId,
                (preference, customers) => PromoCodeMapper.MapFromModel(request, preference, customers));
        }
    }
}
