namespace Vanguard_Engine.DTOs.Users;

public record UserDto(int Id, string Username, string Email, string? Address, int RoleId, DateTime LastLogin);
