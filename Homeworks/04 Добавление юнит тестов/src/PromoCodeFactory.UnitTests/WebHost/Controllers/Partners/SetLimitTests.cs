using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.Core.Exceptions;
using PromoCodeFactory.WebHost.Controllers;
using PromoCodeFactory.WebHost.Models.Partners;
using Soenneker.Utils.AutoBogus;

namespace PromoCodeFactory.UnitTests.WebHost.Controllers.Partners;

public class SetLimitTests
{
    private readonly Mock<IRepository<Partner>> _partnersRepositoryMock;
    private readonly Mock<IRepository<PartnerPromoCodeLimit>> _partnerLimitsRepositoryMock;
    private readonly PartnersController _sut;

    public SetLimitTests()
    {
        _partnersRepositoryMock = new Mock<IRepository<Partner>>();
        _partnerLimitsRepositoryMock = new Mock<IRepository<PartnerPromoCodeLimit>>();
        _sut = new PartnersController(_partnersRepositoryMock.Object, _partnerLimitsRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateLimit_WhenPartnerNotFound_ReturnsNotFound()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        _partnersRepositoryMock
            .Setup(r => r.GetById(partnerId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner?)null);

        // Act
        var result = await _sut.CreateLimit(partnerId, new PartnerPromoCodeLimitCreateRequest(DateTimeOffset.UtcNow.AddDays(1), 1), CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = (NotFoundObjectResult)result.Result;
        notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        notFoundResult.Value.Should().BeOfType<ProblemDetails>();
        var problemDetails = (ProblemDetails)notFoundResult.Value!;
        problemDetails.Title.Should().Be("Partner not found");
    }

    [Fact]
    public async Task CreateLimit_WhenPartnerBlocked_ReturnsUnprocessableEntity()
    {
        // Arrange
        var partner = CreatePartnerWithLimit(Guid.Empty, "Заблокирован", false);
        _partnersRepositoryMock
            .Setup(r => r.GetById(partner.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        // Act
        var result = await _sut.CreateLimit(partner.Id, new PartnerPromoCodeLimitCreateRequest(DateTimeOffset.UtcNow.AddDays(1), 1), CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        var unprocessableResult = (UnprocessableEntityObjectResult)result.Result;
        unprocessableResult.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        unprocessableResult.Value.Should().BeOfType<ProblemDetails>();
        var problemDetails = (ProblemDetails)unprocessableResult.Value!;
        problemDetails.Title.Should().Be("Partner blocked");
    }

    [Fact]
    public async Task CreateLimit_WhenValidRequest_ReturnsCreatedAndAddsLimit()
    {
        // Arrange
        var partner = CreatePartnerWithLimit(Guid.NewGuid(), "Вновь созданный", true);
        _partnersRepositoryMock
            .Setup(r => r.GetById(partner.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);
        const int limits = 2;
        DateTimeOffset endAt = DateTimeOffset.UtcNow.AddDays(3);

        // Act
        var result = await _sut.CreateLimit(partner.Id, new PartnerPromoCodeLimitCreateRequest(endAt, limits), CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = (CreatedAtActionResult)result.Result;
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        createdResult.Value.Should().BeOfType<PartnerPromoCodeLimitResponse>();
        var response = (PartnerPromoCodeLimitResponse)createdResult.Value!;
        response.Limit.Should().Be(limits);
        response.CanceledAt.Should().BeNull();
        response.EndAt.Should().Be(endAt);
    }

    [Fact]
    public async Task CreateLimit_WhenValidRequestWithActiveLimits_CancelsOldLimitsAndAddsNew()
    {
        // Arrange
        var partner = CreatePartnerWithLimit(Guid.NewGuid(), "Отменим старый лимит", true);
        _partnersRepositoryMock
            .Setup(r => r.GetById(partner.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);
        var oldLimit = partner.PartnerLimits.First();

        // Act
        await _sut.CreateLimit(partner.Id, new PartnerPromoCodeLimitCreateRequest(DateTimeOffset.UtcNow.AddDays(2), 1), CancellationToken.None);

        // Assert
        oldLimit.CanceledAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateLimit_WhenUpdateThrowsEntityNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var partner = CreatePartnerWithLimit(Guid.NewGuid(), "Test Update for not found", true);
        _partnersRepositoryMock
            .Setup(r => r.GetById(partner.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);
        _partnersRepositoryMock
            .Setup(r => r.Update(partner, It.IsAny<CancellationToken>()))
            .Throws(new EntityNotFoundException(partner.GetType(), partner.Id));

        // Act
        var result = await _sut.CreateLimit(partner.Id, new PartnerPromoCodeLimitCreateRequest(DateTimeOffset.UtcNow.AddDays(2), 1), CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
        var notFoundResult = (NotFoundResult)result.Result;
        notFoundResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    private static Partner CreatePartnerWithLimit(Guid partnerId, string name, bool isActive, DateTimeOffset? canceledAt = null)
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
            .Generate();

        limits.Add(limit);
        return partner;
    }

}
