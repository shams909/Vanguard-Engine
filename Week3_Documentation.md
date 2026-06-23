# Vanguard Engine - Week 3 Technical Master Document: Recruitment Workflow

This document provides a detailed breakdown of the **Recruitment Workflow** implemented during **Week 3**. It explains the full lifecycle of a security request, from a Client creating a post to a Guard being assigned and eventually completing the work.

---

## 1. Overview of the Recruitment Workflow

The Recruitment Workflow is the core system that matches Client demands for security personnel with available Guard operators.

The workflow follows this lifecycle:
1. **Creation**: Client creates a Recruitment Post (Client Request).
2. **Visibility**: Post becomes visible to Admins (for review) and Guards (for applying).
3. **Application**: Guard applies to the Recruitment Post.
4. **Review**: Client reviews applications and accepts or rejects them.
5. **Assignment**: Upon acceptance, the Guard's status updates to `Busy`, and they are assigned to the perimeter.
6. **Completion**: When the work concludes, an Admin completes the assignment, releasing the Guard back to `Available`.

---

## 2. Technical Implementation Details

### A. Client Request (Recruitment Post) Creation
*   **Location**: `Controllers/ClientRequestController.cs` ➔ `Create()`
*   **Service**: `Services/ClientRequestService.cs` ➔ `CreateRequestAsync()`
*   **Process**:
    *   Client submits a form specifying the Location, Duration, and Number of Guards.
    *   The system validates the input.
    *   The request is stored in the Appwrite `client_requests` collection with an initial status of `Pending`.

### B. Visibility & Job Board
*   **Location**: `Controllers/ClientRequestController.cs` ➔ `OpenJobs()` and `AdminRequests()`
*   **Process**:
    *   **Admin View**: Admins can see all requests via `AdminRequests()`.
    *   **Guard View**: Guards see approved/open requests on the Jobs Board via `OpenJobs()`. The system queries the Guard's current `GuardStatus` to determine if they are eligible to apply.

### C. Guard Application Process
*   **Location**: `Controllers/ClientRequestController.cs` ➔ `Apply()`
*   **Service**: `Services/GuardApplicationService.cs` ➔ `ApplyToJobAsync()`
*   **Process**:
    *   Before applying, the system verifies the Guard's availability. If `GuardStatus == "Busy"`, the application is blocked to prevent double-booking.
    *   If available, a new document is created in the `guard_applications` collection. The `jobId` field links the application to the specific Client Request.
    *   The application status is set to `Pending`.

### D. Client Review & Acceptance
*   **Location**: `Controllers/ClientRequestController.cs` ➔ `AcceptApplication()` / `RejectApplication()`
*   **Service**: `Services/GuardApplicationService.cs` ➔ `AcceptJobApplicationAsync()`
*   **Process**:
    *   The Client reviews the list of applicants under their "My Requests" dashboard.
    *   If **Accepted**:
        *   The application status changes from `Pending` to `Accepted`.
        *   The Guard's global `GuardStatus` updates from `Available` to `Busy`.
        *   The Guard is added to the assigned list for that Client Request.
    *   If **Rejected**: The application is marked as `Rejected` or deleted, allowing the Client to evaluate other candidates.

### E. Current Work & Guard Dashboard
*   **Location**: `Controllers/DashboardController.cs`
*   **Process**:
    *   When the Guard logs in, the dashboard checks their `GuardStatus`.
    *   If `Busy`, the "Current Work" widget dynamically renders their active assignment details (Client, Location, Duration).
    *   The Guard is restricted from applying to any new jobs while `Busy`.

### F. Assignment Completion
*   **Location**: `Controllers/ClientRequestController.cs` ➔ `Complete()`
*   **Service**: `Services/GuardApplicationService.cs` ➔ `CompleteJobAsync()`
*   **Process**:
    *   Once the shift or contract ends, an Admin marks the request as `Completed`.
    *   The system iterates through all guards assigned to the job and resets their `GuardStatus` from `Busy` back to `Available`.
    *   Guards are immediately eligible to apply for new recruitment posts.

---

## 3. Database Validation & Anti-Forgery
All interactions are heavily validated at the Controller and Service levels:
*   **Role-Based Access**: Uses `[Authorize(Roles = "...")]` to ensure Clients cannot browse other clients' posts, and Guards cannot access Admin assignment controls.
*   **Anti-Forgery**: `[ValidateAntiForgeryToken]` is enforced on all POST operations.
*   **Duplicate Prevention**: The system checks existing `guard_applications` for a matching `userId` and `jobId` before allowing a new application.
*   **State Integrity**: State transitions (e.g., `Available` to `Busy`) are locked and atomic via the Appwrite service layer.

---

## 4. MCP & Appwrite Integration
The entire schema, including `guard_applications` and `client_requests`, was configured and managed using the Appwrite API.
*   No Entity Framework migrations are needed.
*   The Unit of Work (`IUnitOfWork`) pattern abstracts the Appwrite Database SDK, allowing `ClientRequestService` and `GuardApplicationService` to cleanly handle domain logic without leaking database queries into the controllers.
