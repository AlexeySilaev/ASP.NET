using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Controllers;
using PromoCodeFactory.WebHost.Models.PromoCodes;
using Soenneker.Utils.AutoBogus;
using System.Linq.Expressions;

namespace PromoCodeFactory.UnitTests.WebHost.Controllers.PromoCodes;

public class CreateTests
{
    private readonly Mock<IRepository<PromoCode>> _promoCodesRepositoryMock;
    private readonly Mock<IRepository<Customer>> _customersRepositoryMock;
    private readonly Mock<IRepository<CustomerPromoCode>> _customerPromoCodesRepositoryMock;
    private readonly Mock<IRepository<Partner>> _partnersRepositoryMock;
    private readonly Mock<IRepository<Preference>> _preferencesRepositoryMock;

    private readonly PromoCodesController _sut;

    public CreateTests()
    {
        _promoCodesRepositoryMock = new Mock<IRepository<PromoCode>>();
        _customersRepositoryMock = new Mock<IRepository<Customer>>();
        _customerPromoCodesRepositoryMock = new Mock<IRepository<CustomerPromoCode>>();
        _partnersRepositoryMock = new Mock<IRepository<Partner>>();
        _preferencesRepositoryMock = new Mock<IRepository<Preference>>();
        _sut = new PromoCodesController(_promoCodesRepositoryMock.Object, _customersRepositoryMock.Object,
            _customerPromoCodesRepositoryMock.Object, _partnersRepositoryMock.Object, _preferencesRepositoryMock.Object);
    }

    [Fact]
    public async Task Create_WhenPartnerNotFound_ReturnsNotFound()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        _partnersRepositoryMock
            .Setup(r => r.GetById(partnerId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner?)null);

        // Act
        var result = await _sut.Create(CreatePromoCodeCreateRequest(partnerId), CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = (NotFoundObjectResult)result.Result;
        notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        notFoundResult.Value.Should().BeOfType<ProblemDetails>();
        var problemDetails = (ProblemDetails)notFoundResult.Value!;
        problemDetails.Title.Should().Be("Partner not found");
    }

    [Fact]
    public async Task Create_WhenPreferenceNotFound_ReturnsNotFound()
    {
        // Arrange
        var partner = CreatePartnerWithLimit(Guid.NewGuid(), "Mock Partner", true);
        _partnersRepositoryMock
            .Setup(r => r.GetById(partner.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        Guid preferenceId = Guid.NewGuid();
        _preferencesRepositoryMock
            .Setup(r => r.GetById(preferenceId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Preference?)null);

        // Act
        var result = await _sut.Create(CreatePromoCodeCreateRequest(partner.Id), CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = (NotFoundObjectResult)result.Result;
        notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        notFoundResult.Value.Should().BeOfType<ProblemDetails>();
        var problemDetails = (ProblemDetails)notFoundResult.Value!;
        problemDetails.Title.Should().Be("Preference not found");
    }

    [Fact]
    public async Task Create_WhenNoActiveLimit_ReturnsUnprocessableEntity()
    {
        // Arrange
        var partner = CreatePartnerWithLimit(Guid.NewGuid(), "Mock Partner", true, DateTimeOffset.UtcNow);
        _partnersRepositoryMock
            .Setup(r => r.GetById(partner.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        var request = CreatePromoCodeCreateRequest(partner.Id);
        _preferencesRepositoryMock
            .Setup(r => r.GetById(request.PreferenceId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Preference { Name = "Mock Preference" });

        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        var objectResult = (ObjectResult)result.Result!;
        objectResult.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        objectResult.Value.Should().BeOfType<ProblemDetails>();
        var problemDetails = (ProblemDetails)objectResult.Value!;
        problemDetails.Title.Should().Be("No active limit");
    }

    [Fact]
    public async Task Create_WhenLimitExceeded_ReturnsUnprocessableEntity()
    {
        // Arrange
        var partner = CreatePartnerWithLimit(Guid.NewGuid(), "Mock Partner", true);
        _partnersRepositoryMock
            .Setup(r => r.GetById(partner.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        var request = CreatePromoCodeCreateRequest(partner.Id);
        _preferencesRepositoryMock
            .Setup(r => r.GetById(request.PreferenceId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Preference { Name = "Mock Preference" });

        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        var objectResult = (ObjectResult)result.Result!;
        objectResult.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        objectResult.Value.Should().BeOfType<ProblemDetails>();
        var problemDetails = (ProblemDetails)objectResult.Value!;
        problemDetails.Title.Should().Be("Limit exceeded");
    }

    [Fact]
    public async Task Create_WhenValidRequest_ReturnsCreatedAndIncrementsIssuedCount()
    {
        // Arrange
        var partner = CreatePartnerWithLimit(Guid.NewGuid(), "Mock Partner", true, null, 0);
        _partnersRepositoryMock
            .Setup(r => r.GetById(partner.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        var request = CreatePromoCodeCreateRequest(partner.Id);
        var preference = new Preference { Name = "Mock Preference", Id =request.PreferenceId };
        _preferencesRepositoryMock
            .Setup(r => r.GetById(request.PreferenceId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(preference);
        _customersRepositoryMock
            .Setup(r => r.GetWhere(It.IsAny<Expression<Func<Customer, bool>>>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new Customer { FirstName = "F", LastName = "L", Email = "none@none.com", Preferences = new[] { preference } } });

        var issuedCount = partner.PartnerLimits.First().IssuedCount;
        // Act
        var result = await _sut.Create(request, CancellationToken.None);

        // Assert
        var createdResult = (CreatedAtActionResult)result.Result!;
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);

        PromoCodeShortResponse response = (PromoCodeShortResponse)createdResult.Value!;
        response.PartnerId.Should().Be(partner.Id);
        response.PreferenceId.Should().Be(preference.Id);
        response.Code.Should().Be(request.Code);
        response.ServiceInfo.Should().Be(request.ServiceInfo);
        response.BeginDate.Should().Be(request.BeginDate);
        response.EndDate.Should().Be(request.EndDate);

        partner.PartnerLimits.First().IssuedCount.Should().Be(issuedCount + 1);
    }

    private static PromoCodeCreateRequest CreatePromoCodeCreateRequest(Guid partnerId)
    {
        return new PromoCodeCreateRequest("mockCode", "mockServiceInfo", partnerId,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(20), Guid.NewGuid());
    }

    private static Partner CreatePartnerWithLimit(Guid partnerId, string name, bool isActive, DateTimeOffset? canceledAt = null, int issuedCpount = 5)
    {
        var role = new AutoFaker<Role>()
            .RuleFor(r => r.Id, _ => Guid.NewGuid())
            .Generate();

        var employee = new AutoFaker<Employee>()
            .RuleFor(e => e.Id, _ => Guid.NewGuid())
            .RuleFor(e => e.Role, role)
            .Generate();

        var limits = new List<PartnerPromoCodeLimit>();
        var partner = new AutoFaker<Partner>()
            .RuleFor(p => p.Id, _ => partnerId)
            .RuleFor(p => p.Name, _ => name)
            .RuleFor(p => p.IsActive, _ => isActive)
            .RuleFor(p => p.Manager, employee)
            .RuleFor(p => p.PartnerLimits, limits)
            .Generate();

        var limit = new AutoFaker<PartnerPromoCodeLimit>()
            .RuleFor(l => l.Id, _ => Guid.NewGuid())
            .RuleFor(l => l.Partner, partner)
            .RuleFor(l => l.CanceledAt, _ => canceledAt)
            .RuleFor(l => l.CreatedAt, _ => DateTimeOffset.UtcNow.AddDays(-1))
            .RuleFor(l => l.EndAt, _ => DateTimeOffset.UtcNow.AddDays(30))
            .RuleFor(l => l.IssuedCount, _ => issuedCpount)
            .RuleFor(l => l.Limit, _ => 5)
            .Generate();

        limits.Add(limit);
        return partner;
    }

}
