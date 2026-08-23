using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.WebHost.Models;

namespace Pcf.GivingToCustomer.WebHost.Controllers
{
    /// <summary>
    /// Клиенты
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CustomersController
        : ControllerBase
    {
        private readonly CustomerService _customerService;

        public CustomersController(CustomerService customerService)
        {
            _customerService = customerService;
        }
        
        /// <summary>
        /// Получить список клиентов
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<CustomerShortResponse>>> GetCustomersAsync()
        {
            var customers =  await _customerService.GetAllAsync();

            var response = customers.Select(x => new CustomerShortResponse()
            {
                Id = x.Id,
                Email = x.Email,
                FirstName = x.FirstName,
                LastName = x.LastName
            }).ToList();

            return Ok(response);
        }
        
        /// <summary>
        /// Получить клиента по id
        /// </summary>
        /// <param name="id">Id клиента, например <example>a6c8c6b1-4349-45b0-ab31-244740aaf0f0</example></param>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CustomerResponse>> GetCustomerAsync(Guid id)
        {
            var customer =  await _customerService.GetByIdAsync(id);

            var response = new CustomerResponse(customer);

            return Ok(response);
        }
        
        /// <summary>
        /// Создать нового клиента
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<CustomerResponse>> CreateCustomerAsync(CreateOrEditCustomerRequest request)
        {
            Customer customer = await _customerService.AddAsync(request);
            return CreatedAtAction(nameof(GetCustomerAsync), new {id = customer.Id}, customer.Id);
        }
        
        /// <summary>
        /// Обновить клиента
        /// </summary>
        /// <param name="id">Id клиента, например <example>a6c8c6b1-4349-45b0-ab31-244740aaf0f0</example></param>
        /// <param name="request">Данные запроса></param>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> EditCustomersAsync(Guid id, CreateOrEditCustomerRequest request)
        {
            bool result = await _customerService.UpdateAsync(id, request);
            
            if (result == false)
                return NotFound();
            return NoContent();
        }
        
        /// <summary>
        /// Удалить клиента
        /// </summary>
        /// <param name="id">Id клиента, например <example>a6c8c6b1-4349-45b0-ab31-244740aaf0f0</example></param>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCustomerAsync(Guid id)
        {
            bool result = await _customerService.DeleteAsync(id);
            
            if (result == false)
                return NotFound();

            return NoContent();
        }
    }
}