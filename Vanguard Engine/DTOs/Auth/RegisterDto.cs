namespace Vanguard_Engine.DTOs.Auth;

public record RegisterDto(string Username, string Email, string Password, string? Address, int RoleId);
