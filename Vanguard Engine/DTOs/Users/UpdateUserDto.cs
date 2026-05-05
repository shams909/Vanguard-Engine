namespace Vanguard_Engine.DTOs.Users;

public record UpdateUserDto(string Username, string Email, string? Password, string? Address, int RoleId, DateTime LastLogin);
