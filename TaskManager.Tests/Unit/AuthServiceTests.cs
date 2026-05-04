using System.IdentityModel.Tokens.Jwt;
using BCrypt.Net;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Tests.Unit;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private Mock<ILogger<AuthService>> _loggerMock;
    private IConfiguration _configuration;
    private AuthService _sut;

    [SetUp]
    public void SetUp()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        var inMemorySettings = new Dictionary<string, string?>
        {
            { "JWT:Secret", "TestSecret_SuperSecretKey_2026!!_XYZ987_Test" },
            { "JWT:Issuer", "TaskManagerApi" },
            { "JWT:Audience", "TaskManagerApiUsers" },
            { "JWT:ExpirationInMinutes", "60" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _sut = new AuthService(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _configuration,
            _loggerMock.Object);
    }

    [Test]
    public async Task RegisterAsync_WithValidData_ShouldReturnTokenAndAuthResponse()
    {
        var request = new RegisterRequest("test@example.com", "password123");

        _userRepositoryMock.Setup(r => r.ExistsAsync(request.Email)).ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) =>
            {
                u.Id = 1;
                return u;
            });
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.RegisterAsync(request);

        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.Email.Should().Be(request.Email);
        result.UserId.Should().Be(1);
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(result.Token).Should().BeTrue();

        _userRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RegisterAsync_WithDuplicateEmail_ShouldThrowConflictException()
    {
        var request = new RegisterRequest("duplicate@example.com", "password123");

        _userRepositoryMock.Setup(r => r.ExistsAsync(request.Email)).ReturnsAsync(true);

        Func<Task> act = async () => await _sut.RegisterAsync(request);

        await act.Should().ThrowAsync<ConflictException>();
        _userRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnTokenAndAuthResponse()
    {
        var password = "password123";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User
        {
            Id = 1,
            Email = "user@example.com",
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        var request = new LoginRequest("user@example.com", password);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync(user);

        var result = await _sut.LoginAsync(request);

        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.Email.Should().Be(user.Email);
        result.UserId.Should().Be(user.Id);
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Test]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowUnauthorizedException()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        var user = new User
        {
            Id = 1,
            Email = "user@example.com",
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        var request = new LoginRequest("user@example.com", "wrongpassword");
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync(user);

        Func<Task> act = async () => await _sut.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Test]
    public async Task LoginAsync_WithNonExistentEmail_ShouldThrowUnauthorizedException()
    {
        var request = new LoginRequest("nonexistent@example.com", "password123");
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync((User?)null);

        Func<Task> act = async () => await _sut.LoginAsync(request);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Test]
    public async Task RegisterAsync_PasswordShouldBeHashedWithBCrypt()
    {
        var request = new RegisterRequest("hash@example.com", "password123");
        User? capturedUser = null;

        _userRepositoryMock.Setup(r => r.ExistsAsync(request.Email)).ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .ReturnsAsync((User u) =>
            {
                u.Id = 2;
                return u;
            });
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _sut.RegisterAsync(request);

        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().NotBe(request.Password);
        BCrypt.Net.BCrypt.Verify(request.Password, capturedUser.PasswordHash).Should().BeTrue();
    }
}
