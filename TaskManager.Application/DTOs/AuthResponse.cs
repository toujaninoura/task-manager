namespace TaskManager.Application.DTOs;

public record AuthResponse(string Token, string Email, int UserId, DateTime ExpiresAt);
