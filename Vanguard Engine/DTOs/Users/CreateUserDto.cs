namespace Vanguard_Engine.DTOs.Users;

public record CreateUserDto(string Username, string Email, string Password, string? Address, string? RoleId, DateTime LastLogin);
