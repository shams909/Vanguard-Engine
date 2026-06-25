# Live Real-Time Notification System

## Overview
The Vanguard Engine uses **SignalR** (ASP.NET Core WebSockets) to deliver instant, Instagram-style push notifications to every user on the platform without requiring a page refresh. Notifications are both **persisted to the Appwrite database** (so they survive a browser refresh) and **pushed live** to the recipient's browser the moment an action occurs.

---

## Why SignalR and Not Hangfire?

| Feature | SignalR | Hangfire |
|---|---|---|
| **Push speed** | Instant (< 100ms) | Delayed (scheduled jobs) |
| **Real-time bell popup** | ✅ Yes | ❌ No |
| **Two-way communication** | ✅ Yes | ❌ No |
| **Use case** | Live chats, feeds, alerts | Email retries, cron jobs |

**Hangfire** is excellent for background retry queues and scheduled tasks (e.g. sending an email 24 hours after signup). For **live user-to-user push alerts**, SignalR is the correct and industry-standard solution — used by Microsoft Teams, Discord, and similar platforms.

---

## Architecture

### 1. The Hub — `Hubs/NotificationHub.cs`
The SignalR Hub is the WebSocket endpoint. When a browser loads any page, it connects to this hub and **joins a group named after its own User ID**. This enables perfectly targeted personal notifications.

```csharp
public class NotificationHub : Hub
{
    // Client calls this after connecting to subscribe to their personal channel
    public async Task Subscribe(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, userId);
    }
}
```

### 2. The Service — `Services/NotificationService.cs`
`NotificationService` is injected into any Controller that needs to push a notification. It does two things in one call:
1. **Persists** the notification as a document in the Appwrite `notifications` collection.
2. **Pushes** it live to the recipient's browser via `IHubContext<NotificationHub>`.

```csharp
// Send to a single specific user
await _notificationService.CreateNotificationAsync(
    userId: guardId,
    title: "You Received a New Rating",
    message: "A client rated your service 5/5 ★★★★★",
    type: "Info"
);

// Send to ALL users with a specific role (e.g. all Admins at once)
await _notificationService.NotifyRoleAsync(
    roleName: "Admin",
    title: "New Patrol Request",
    message: "John Client submitted a new guard patrol request.",
    type: "Info"
);
```

### 3. The Frontend — `wwwroot/js/notification.js`
On every page load, the browser:
1. Establishes a persistent WebSocket connection to `/notificationHub`.
2. Reads the current user's ID from a `<meta>` tag in the layout.
3. Calls `hub.invoke("Subscribe", userId)` to join the personal channel.
4. Listens for `ReceiveNotification` events and immediately renders a toast + increments the bell badge count.

---

## Notification Trigger Map

Every major user action now triggers an instant push notification to the right recipients:

| Action | Triggered By | Who Gets Notified | Type |
|---|---|---|---|
| Submit Incident / Complaint | Guard or Client | All Admins | ⚠️ Warning |
| Resolve Incident | Admin | Original Reporter (Guard/Client) | ℹ️ Info |
| Submit Guard Rating | Client | Rated Guard | ℹ️ Info |
| Submit Guard Rating | Client | All Admins (summary) | ℹ️ Info |
| Submit New Patrol Request | Client | All Admins | ℹ️ Info |
| Update Request Status | Admin | The requesting Client | ℹ️ Info / ⚠️ Warning |
| Guard Check-In | Guard | All Admins | ℹ️ Info |
| Guard Check-Out | Guard | All Admins | ℹ️ Info |

---

## Appwrite Database Schema — `notifications` Collection

Each notification is stored as a document with the following attributes:

| Attribute | Type | Description |
|---|---|---|
| `userId` | string | The recipient's User ID |
| `title` | string | Short notification title |
| `message` | string | Full notification body |
| `type` | string | `Info`, `Warning`, or `Critical` |
| `isRead` | boolean | `false` by default, set to `true` when opened |
| `expiration` | datetime | Auto-expire after 30 days |
| `$createdAt` | datetime | Appwrite auto-generated timestamp |

---

## Data Flow — Step by Step

```
User Action (e.g. Guard submits incident)
        ↓
GuardShiftController / IncidentController / etc.
        ↓
INotificationService.NotifyRoleAsync("Admin", ...)
        ↓
        ├──► 1. Persist to Appwrite `notifications` collection
        └──► 2. IHubContext<NotificationHub>.Clients.Group(userId).SendAsync("ReceiveNotification", payload)
                        ↓
             Admin's browser WebSocket connection receives event
                        ↓
             notification.js renders live toast + bell badge increments
```

---

## Notification Bell UI
The notification bell icon in the top navigation bar is powered by:
- **Real-time badge counter**: Increments live via SignalR without page refresh.
- **Slide-out sidebar drawer**: Opens on bell click, listing all unread + read notifications.
- **Mark as Read**: Each notification can be individually marked as read via the `NotificationController`.
- **30-day expiry**: Old notifications are automatically excluded from display after 30 days.

---

## Verification
- Build compiled with **0 warnings and 0 errors** (`dotnet build`).
- SignalR hub is registered in `Program.cs` via `app.MapHub<NotificationHub>("/notificationHub")`.
- `INotificationService` is registered as `Scoped` in the DI container.

The notification system is production-ready and scales naturally with the SignalR group-based architecture.
