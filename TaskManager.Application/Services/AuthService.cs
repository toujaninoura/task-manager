using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var validation = await _registerValidator.ValidateAsync(request);
        if (!validation.IsValid)
            throw new Domain.Exceptions.ValidationException(validation.Errors.Select(e => e.ErrorMessage));

        _logger.LogInformation("Registering user with email {Email}", request.Email);

        var exists = await _userRepository.ExistsAsync(request.Email);
        if (exists)
            throw new ConflictException($"A user with email '{request.Email}' already exists.");

        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User {Email} registered successfully with Id {UserId}", user.Email, user.Id);

        var (token, expiresAt) = _tokenService.GenerateToken(user.Id, user.Email);
        return new AuthResponse(token, user.Email, user.Id, expiresAt);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var validation = await _loginValidator.ValidateAsync(request);
        if (!validation.IsValid)
            throw new Domain.Exceptions.ValidationException(validation.Errors.Select(e => e.ErrorMessage));

        _logger.LogInformation("Login attempt for email {Email}", request.Email);

        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        _logger.LogInformation("User {Email} logged in successfully", user.Email);

        var (token, expiresAt) = _tokenService.GenerateToken(user.Id, user.Email);
        return new AuthResponse(token, user.Email, user.Id, expiresAt);
    }
}
