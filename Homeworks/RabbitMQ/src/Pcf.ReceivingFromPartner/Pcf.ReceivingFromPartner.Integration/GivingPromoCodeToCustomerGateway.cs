using MassTransit;
using Microsoft.Extensions.Configuration;
using Pcf.ReceivingFromPartner.Core.Abstractions.Gateways;
using Pcf.ReceivingFromPartner.Core.Domain;
using Pcf.ReceivingFromPartner.Integration.Dto;
using SharedModels;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pcf.ReceivingFromPartner.Integration
{
    public class GivingPromoCodeToCustomerGateway
        : IGivingPromoCodeToCustomerGateway
    {
        private readonly HttpClient _httpClient;
        private readonly IBus _bus;
        private readonly bool _useRabbitMQ;

        public GivingPromoCodeToCustomerGateway(HttpClient httpClient, IBus bus, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _bus = bus;
            _useRabbitMQ = Convert.ToBoolean(configuration["CallingSettings:UseRabbitMQ"]);
        }

        public async Task GivePromoCodeToCustomer(PromoCode promoCode)
        {
            var dto = new GivePromoCodeToCustomerDto()
            {
                PartnerId = promoCode.Partner.Id,
                BeginDate = promoCode.BeginDate.ToShortDateString(),
                EndDate = promoCode.EndDate.ToShortDateString(),
                PreferenceId = promoCode.PreferenceId,
                PromoCode = promoCode.Code,
                ServiceInfo = promoCode.ServiceInfo,
                PartnerManagerId = promoCode.PartnerManagerId
            };

            if (_useRabbitMQ)
            {
                await _bus.Publish<IGivePromoCodeToCustomerDto>(dto);
            }
            else
            {
                var response = await _httpClient.PostAsJsonAsync("api/v1/promocodes", dto);
                response.EnsureSuccessStatusCode();
            }
        }
    }
}