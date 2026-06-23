# Vanguard-Engine: Week 5 Documentation

## Overview

In Week 5, we completed the final crucial pieces of our robust Shift & Attendance system. While much of the foundational attendance work (such as check-ins and the Admin Attendance Dashboard) was already in place, this week focused on **linking active attendance records with pre-scheduled shifts** and **validating time-based actions** to prevent unauthorized check-ins.

---

## 1. Database & Schema Linkage

### Updating Appwrite
We modified the `AppwriteProvisioner/Program.cs` file to automatically deploy a new string attribute called `assignedShiftId` to the existing `guard_shifts` collection.
- This creates a relational bridge between the **Attendance Record** (`guard_shifts`) and the **Future Schedule** (`assigned_shifts`).

### Entity Model Updates (`GuardShift.cs`)
- Added the `AssignedShiftId` property and mapped it to the new Appwrite schema. This guarantees that when a guard checks in, the ID of their scheduled shift is permanently tied to their attendance log.

---

## 2. Business Logic & Time Validation

### Strict Check-In Rules (`GuardShiftService.cs -> CheckInAsync`)
We overhauled the logic to enforce exact time-based constraints:
1. **Date Validation:** The service cross-references the guard's Assigned Shifts and explicitly checks if the shift is scheduled for **today**. If the guard tries to check into a shift scheduled for tomorrow, they are blocked.
2. **Time Validation (30-Minute Buffer):** The service reads the specific `StartTime` of the shift (e.g., `14:00`). Guards are only allowed to clock in up to **30 minutes early**. If they arrive 3 hours early and try to check in, the server denies the request with a detailed message.
3. **Status Syncing:** Upon a successful check-in, the system automatically tags the underlying `AssignedShift` as "Active".

### Synchronized Check-Out (`GuardShiftService.cs -> CheckOutAsync`)
- Once a guard completes their deployment and checks out, their `GuardShift` record calculates their total duration for payroll.
- The service now seamlessly syncs with the schedule by locating the linked `AssignedShift` and updating its status to "Completed".

---

## 3. UI & Controller Updates

### Dashboard Controller (`DashboardController.cs`)
- Added `IAssignedShiftService` to the Dependency Injection framework (in both the controller and `Program.cs`) to fetch the guard's scheduled shifts.
- The backend identifies the guard's **"Today's Shift"** and passes it securely to the Razor view (`ViewBag.TodaysShift`).

### Tactical Guard Console (`Guard.cshtml`)
The Operator Terminal UI was heavily updated to be context-aware:
- **Scheduled Today:** If the guard has a shift today, a green "Scheduled Shift Available" widget appears, telling them their exact hours, and enables the Check-In button securely bundled with the `assignedShiftId`.
- **Not Scheduled:** If they have no shift today, the system displays a prominent "No Shift Scheduled Today" message and strictly disables the Check-In button, graying it out to prevent unauthorized or accidental clicks.

---

## 4. How to Verify & Test

1. **Provision Database:** Run the `AppwriteProvisioner` to ensure the `assignedShiftId` attribute is safely added to your Appwrite Cloud.
2. **Test Blocked Check-In:** Use the Admin panel to assign a shift to a guard for **tomorrow**. Log in as that guard; verify the Check-In button is disabled and reads "No Shift Scheduled Today".
3. **Test Early Check-In:** Admin assigns a shift for today, but starting 5 hours from now. Log in as the guard and attempt to check in; verify the backend throws the "You are too early" validation error.
4. **Test Valid Flow:** Update the shift to start right now. Log in as the guard, click Check-In, wait a minute, and click Check-Out. Finally, log in as an Admin and verify that the shift on the Shift Management panel successfully moved to the "Completed" state!
