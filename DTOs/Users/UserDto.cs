namespace Vanguard_Engine.DTOs.Users;

public record UserDto(string Id, string Username, string Email, string? Address, string? RoleId, DateTime LastLogin);
