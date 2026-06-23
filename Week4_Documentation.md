# Vanguard-Engine: Week 4 Documentation

## Overview

In Week 4, we implemented the **Tactical Shift Assignment Module**. This system allows administrators to proactively schedule guards for future shifts while ensuring there are no overlapping time conflicts. Operators (guards) can view their assigned deployments directly from their console.

---

## 1. Architectural Patterns Applied

- **Generic Repository Pattern**: Maintained DRY principles by extending our existing `AppwriteRepository<T>` to manage the new `AssignedShift` models.
- **Unit of Work Pattern**: Injected the new shift repositories into the unified `IUnitOfWork` pipeline, ensuring our Service layer maintains clean, abstract data access.
- **Server-Side Pagination & Rendering**: Leveraged Appwrite's fast server-side query limitations alongside ASP.NET Core Razor Pages to ensure fast page loads and robust security via HTTP-only cookies and Anti-Forgery tokens.

---

## 2. Database Layer Updates (Appwrite)

### Provisioning the `assigned_shifts` Collection
We updated the `AppwriteProvisioner/Program.cs` to automatically deploy a new database collection called `assigned_shifts`.

**Collection Schema:**
- `guardId` (String, 255, Required)
- `guardName` (String, 255, Required)
- `shiftDate` (String, 100, Required) - Format: YYYY-MM-DD
- `startTime` (String, 50, Required) - Format: HH:mm
- `endTime` (String, 50, Required) - Format: HH:mm
- `status` (String, 50, Required) - e.g., "Scheduled", "Cancelled", "Completed"

---

## 3. Backend Implementation

### A. Entity Model (`AssignedShift.cs`)
Introduced a clean data model explicitly built to separate **Future Scheduled Shifts** from the legacy `GuardShift` model, which acts strictly as a live check-in/attendance logger. 

### B. Data Access Layer
- **`IAssignedShiftRepository` & `AppwriteAssignedShiftRepository`**: Added strongly typed methods like `GetByGuardIdAsync()` and `GetAllAssignedShiftsAsync()`.
- Implemented efficient mapping through the base `MapToEntity()` to safely strip Appwrite metadata tokens (like `$id`) and map them directly to native C# entity properties.

### C. Business Logic Layer (`AssignedShiftService.cs`)
Implemented the `AssignShiftAsync` service method with robust **Conflict Prevention**:
- Validates that `startTime` is always strictly before `endTime`.
- Iterates over existing shifts for the target operator on the specific date.
- Uses string-based time comparison to explicitly block the creation of the shift if `newStartTime < existingEndTime` AND `newEndTime > existingStartTime`.

---

## 4. Frontend Implementation (Razor Views)

### A. Admin Operations Center
- **Shift Management Panel** (`AdminPanel.cshtml`): 
  - Admins can select vetted operators from a dynamic dropdown. 
  - They can specify the Date, Start Time, and End Time to assign a shift.
  - A roster table displays all assigned shifts across the organization, allowing Admins to dynamically update a shift's status (e.g., Canceling a shift if the operator is sick, or restoring it).
- **Dashboard Integration**: Added a "Shift Management" shortcut directly into the `Dashboard/Admin.cshtml` module grid.

### B. Tactical Operator Console (Guard View)
- **Schedule Viewer** (`GuardSchedule.cshtml`): Operators have a dedicated, read-only interface to view all their explicitly scheduled operations, formatted with readable dates and clear status badges.
- **Dashboard Integration**: Added an "Assigned Shifts" mini-widget in `Dashboard/Guard.cshtml` placed prominently below the active VIP mission display, giving operators quick access to their upcoming roster.

---

## 5. How to Verify & Test

1. **Database Setup**: Ensure you run `dotnet run` inside the `AppwriteProvisioner` directory to safely apply the `assigned_shifts` collection to your Appwrite Cloud instance.
2. **Assign a Shift**: Log in with an Admin account, navigate to "Shift Management", and assign an operator a shift (e.g., 09:00 to 17:00).
3. **Trigger Conflict Prevention**: Attempt to assign that exact same operator another shift on the same date from 12:00 to 15:00. The system will throw a styled error message blocking the overlap.
4. **View as Guard**: Log out and log in as the assigned Guard. On your dashboard, click "View Schedule" to see the newly assigned timeslot.
