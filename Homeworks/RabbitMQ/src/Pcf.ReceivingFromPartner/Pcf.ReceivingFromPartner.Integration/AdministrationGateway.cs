using MassTransit;
using Microsoft.Extensions.Configuration;
using Pcf.ReceivingFromPartner.Core.Abstractions.Gateways;
using Pcf.ReceivingFromPartner.Integration.Dto;
using SharedModels;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pcf.ReceivingFromPartner.Integration
{
    public class AdministrationGateway
        : IAdministrationGateway
    {
        private readonly HttpClient _httpClient;
        private readonly IBus _bus;
        private readonly bool _useRabbitMQ;

        public AdministrationGateway(HttpClient httpClient, IBus bus, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _bus = bus;
            _useRabbitMQ = Convert.ToBoolean(configuration["CallingSettings:UseRabbitMQ"]);
        }

        public async Task NotifyAdminAboutPartnerManagerPromoCode(Guid partnerManagerId)
        {

            if (_useRabbitMQ)
                await _bus.Publish<ISupportPartnerManagerId>(new GivePromoCodeToCustomerDto { PartnerManagerId = partnerManagerId });
            else
            {
                var response = await _httpClient.PostAsync($"api/v1/employees/{partnerManagerId}/appliedPromocodes",
                     new StringContent(string.Empty));
                response.EnsureSuccessStatusCode();
            }
        }
    }
}