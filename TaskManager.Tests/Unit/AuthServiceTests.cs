using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
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
    private Mock<IPasswordHasher> _passwordHasherMock;
    private Mock<ITokenService> _tokenServiceMock;
    private Mock<ILogger<AuthService>> _loggerMock;
    private AuthService _sut;

    [SetUp]
    public void SetUp()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenServiceMock = new Mock<ITokenService>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        _sut = new AuthService(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task RegisterAsync_WithValidData_ShouldReturnTokenAndAuthResponse()
    {
        var request = new RegisterRequest("test@example.com", "password123");
        var fakeToken = "fake.jwt.token";
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        _userRepositoryMock.Setup(r => r.ExistsAsync(request.Email)).ReturnsAsync(false);
        _passwordHasherMock.Setup(h => h.Hash(request.Password)).Returns("hashed_password");
        _userRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) =>
            {
                u.Id = 1;
                return u;
            });
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _tokenServiceMock.Setup(t => t.GenerateToken(1, request.Email)).Returns((fakeToken, expiresAt));

        var result = await _sut.RegisterAsync(request);

        result.Should().NotBeNull();
        result.Token.Should().Be(fakeToken);
        result.Email.Should().Be(request.Email);
        result.UserId.Should().Be(1);
        result.ExpiresAt.Should().Be(expiresAt);

        _userRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasherMock.Verify(h => h.Hash(request.Password), Times.Once);
        _tokenServiceMock.Verify(t => t.GenerateToken(1, request.Email), Times.Once);
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
        var user = new User
        {
            Id = 1,
            Email = "user@example.com",
            PasswordHash = "hashed_password",
            CreatedAt = DateTime.UtcNow
        };
        var fakeToken = "fake.jwt.token";
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        var request = new LoginRequest("user@example.com", password);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.Verify(password, user.PasswordHash)).Returns(true);
        _tokenServiceMock.Setup(t => t.GenerateToken(user.Id, user.Email)).Returns((fakeToken, expiresAt));

        var result = await _sut.LoginAsync(request);

        result.Should().NotBeNull();
        result.Token.Should().Be(fakeToken);
        result.Email.Should().Be(user.Email);
        result.UserId.Should().Be(user.Id);
        result.ExpiresAt.Should().Be(expiresAt);
    }

    [Test]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowUnauthorizedException()
    {
        var user = new User
        {
            Id = 1,
            Email = "user@example.com",
            PasswordHash = "hashed_password",
            CreatedAt = DateTime.UtcNow
        };

        var request = new LoginRequest("user@example.com", "wrongpassword");
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.Verify(request.Password, user.PasswordHash)).Returns(false);

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
    public async Task RegisterAsync_PasswordShouldBeHashedViaPasswordHasher()
    {
        var request = new RegisterRequest("hash@example.com", "password123");
        var expectedHash = "bcrypt_hashed_password";
        User? capturedUser = null;
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        _userRepositoryMock.Setup(r => r.ExistsAsync(request.Email)).ReturnsAsync(false);
        _passwordHasherMock.Setup(h => h.Hash(request.Password)).Returns(expectedHash);
        _userRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .ReturnsAsync((User u) =>
            {
                u.Id = 2;
                return u;
            });
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _tokenServiceMock.Setup(t => t.GenerateToken(2, request.Email)).Returns(("token", expiresAt));

        await _sut.RegisterAsync(request);

        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().Be(expectedHash);
        capturedUser.PasswordHash.Should().NotBe(request.Password);
        _passwordHasherMock.Verify(h => h.Hash(request.Password), Times.Once);
    }
}
