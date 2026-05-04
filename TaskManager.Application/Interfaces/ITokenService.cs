namespace TaskManager.Application.Interfaces;

public interface ITokenService
{
    (string token, DateTime expiresAt) GenerateToken(int userId, string email);
}
