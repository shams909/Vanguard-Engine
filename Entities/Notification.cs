namespace Vanguard_Engine.Entities;

using System;

public class Notification
{
    public string Id { get; set; } = string.Empty; // Appwrite document ID
    public string UserId { get; set; } = string.Empty; // Recipient Appwrite user ID
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
    public string Type { get; set; } = "Info"; // Info, Warning, Critical
    public DateTime? Expiration { get; set; }
}
