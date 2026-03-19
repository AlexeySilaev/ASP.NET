using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Mapping;
using PromoCodeFactory.WebHost.Models.PromoCodes;

namespace PromoCodeFactory.WebHost.Controllers;

/// <summary>
/// Промокоды
/// </summary>
public class PromoCodesController(IRepository<PromoCode> promocodesRepository, IRepository<Preference> preferencesRepository,
    IRepository<Employee> employeesRepository, IRepository<Customer> customersRepository) : BaseController
{
    /// <summary>
    /// Получить все промокоды
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PromoCodeShortResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PromoCodeShortResponse>>> Get(CancellationToken ct)
    {
        var promocodes = await promocodesRepository.GetAll(ct: ct);
        var promocodesModels = promocodes.Select(p => PromoCodesMapper.ToPromoCodeShortResponse(p)).ToList();
        return Ok(promocodesModels);
    }

    /// <summary>
    /// Получить промокод по id
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PromoCodeShortResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromoCodeShortResponse>> GetById(Guid id, CancellationToken ct)
    {
        var promocode = await promocodesRepository.GetById(id, true, ct);
        if (promocode is null)
            return NotFound();

        return Ok(PromoCodesMapper.ToPromoCodeShortResponse(promocode));
    }

    /// <summary>
    /// Создать промокод и выдать его клиентам с указанным предпочтением
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PromoCodeShortResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromoCodeShortResponse>> Create(PromoCodeCreateRequest request, CancellationToken ct)
    {
        var preference = await preferencesRepository.GetById(request.PreferenceId);
        if(preference == null)
            return BadRequest(new ProblemDetails { Title = "Invalid preference", Detail = $"Preference with Id {request.PreferenceId} not found." });

        var employee = await employeesRepository.GetById(request.PartnerManagerId);
        if (employee == null)
            return BadRequest(new ProblemDetails { Title = "Invalid employee", Detail = $"Employee with Id {request.PartnerManagerId} not found." });

        if (request.BeginDate > request.EndDate)
            return BadRequest(new ProblemDetails { Title = "Invalid promocode BeginDate/EndDate." });

        var promocode = PromoCodesMapper.ToPromoCode(request, employee, preference);
        await promocodesRepository.Add(promocode, ct);

        // выдача промокода клиентам
        DateTimeOffset createdAt = DateTimeOffset.UtcNow;
        foreach (var customer in preference.Customers)
        {
            customer.CustomerPromoCodes.Add(new CustomerPromoCode
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                PromoCodeId = promocode.Id,
                CreatedAt = createdAt,
            });
            await customersRepository.Update(customer, ct); // здесь накладные расходы на каждое сохрание в базе, как избежать - не знаю
        }

        return CreatedAtAction(nameof(Create), new { id = promocode.Id }, promocode);
    }

    /// <summary>
    /// Применить промокод (отметить, что клиент использовал промокод)
    /// </summary>
    [HttpPost("{id:guid}/apply")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Apply(
        [FromRoute] Guid id,
        [FromBody] PromoCodeApplyRequest request,
        CancellationToken ct)
    {
        var customer = await customersRepository.GetById(request.CustomerId);
        if (customer == null)
            return BadRequest(new ProblemDetails { Title = "Invalid customer", Detail = $"Customer with Id {request.CustomerId} not found." });

        // далее подразумеваю, что параметр id - это идентификатор промокода конкретного клиента
        var customerPromocode = customer.CustomerPromoCodes.SingleOrDefault(c => c.Id == id);
        if (customerPromocode == null)
            return BadRequest(new ProblemDetails { Title = "Invalid customer promocode", Detail = $"Customer promocode with Id {id} not found." });
        if (customerPromocode.AppliedAt != null)
            return BadRequest(new ProblemDetails { Title = "Promocode has been applied." });

        // проверка, что существует промокод с валидным диапазоном применения дат
        var promocode = await promocodesRepository.GetById(customerPromocode.PromoCodeId, true, ct);
        if (promocode == null)
            return NotFound();
        else if (DateTimeOffset.UtcNow < promocode.BeginDate || promocode.EndDate < DateTimeOffset.UtcNow)
            return BadRequest(new ProblemDetails { Title = "Promocode cannot be applied." });

        // применение
        customerPromocode.AppliedAt = DateTimeOffset.UtcNow;
        await customersRepository.Update(customer, ct);

        return NoContent();
    }
}
