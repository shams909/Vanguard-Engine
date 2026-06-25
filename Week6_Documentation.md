# Week 6: Incident & Complaint Management System

## Overview
Week 6 introduces a unified system for tracking operational issues within the Vanguard Engine platform. The new architecture efficiently consolidates **Guard Tactical Incidents** and **Client Service Complaints** into a single, cohesive workflow, allowing the Admin team to review, manage, and resolve tickets securely.

## Key Features Implemented

### 1. Multi-Role Abstraction Logic
Instead of building two redundant systems, Week 6 utilizes a shared, intelligent reporting pipeline that dynamically adapts to the user's role:
- **For Guards:** The UI is branded as **"Incident Reporting"**. Guards use this tool to log on-site security incidents, breaches, and operational disruptions.
- **For Clients:** The UI transforms into **"Service Complaints"**. Clients utilize this space to report dissatisfaction with guard performance or deployment issues.

### 2. Client Portal Updates
- **Complaint Submission Form:** Clients can submit a Title and Detailed Description of their issue.
- **Complaint Tracking:** A dedicated `MyReports` dashboard where clients can see the status of their complaints (`Open` vs `Resolved`) and read any resolution notes left by Vanguard Admins.

### 3. Guard Portal Updates
- **Tactical Incident Logging:** Guards now have a distinct widget on their dashboard to log incidents.
- **Incident History:** Guards can view past reports to see if dispatch/administration has formally resolved their reported field incidents.

### 4. Admin Operations Center
- **Incident Resolution Dashboard:** A robust panel that pulls all incoming tickets across the platform.
- **Live Filtering:** Admins can filter tickets by `All`, `Open`, or `Resolved`.
- **Triage & Resolve Workflow:** Admins can review a ticket, input official `Resolution Notes`, and mark the case as `Resolved`. This permanently locks the ticket and updates the status across the client/guard portals.

---

## Technical Architecture

### Appwrite Database Schema
A new collection was provisioned using the `AppwriteProvisioner`:
- **Collection Name:** `incidents`
- **Attributes:**
  - `reportedByUserId` (string, 255): Link to the reporter.
  - `reportedByName` (string, 255): Cached display name of the reporter.
  - `reporterRole` (string, 50): Tracks if the submitter was a `Guard` or `Client`.
  - `type` (string, 50): Explicitly set as `Incident` or `Complaint`.
  - `title` (string, 255): Subject of the report.
  - `description` (string, 5000): Long-form details of the issue.
  - `status` (string, 50): Starts as `Open`, updates to `Resolved`.
  - `resolutionNotes` (string, 5000, optional): Populated by an admin upon closure.
  - `resolvedByAdminId` (string, 255, optional): Tracks which administrator resolved the ticket.

### Backend Infrastructure
The feature utilizes the platform's standard Repository & Unit of Work pattern:
1. **`Incident.cs` (Entity):** C# Model mapping to the Appwrite NoSQL schema using `[JsonProperty]`.
2. **`IIncidentRepository` / `AppwriteIncidentRepository`:** Handles CRUD and specific status/reporter queries via the Appwrite SDK.
3. **`IncidentService.cs`:** Contains the core business logic, including validation, default value assignment, and resolution timestamping.
4. **`IncidentController.cs`:** Manages the routing, authorization, and view delivery.

### Security & Validation
- **Strict Role Authorization:** The Controller actions are locked behind `[Authorize(Roles = "...")]` to ensure zero unauthorized access.
- **Tamper-Proof Role Assignment:** The `reporterRole` and `type` fields are completely detached from the frontend form. The Controller pulls this data securely from the server-side JWT / Cookie Claims (`ClaimTypes.Role`).
- **Anti-Forgery:** All POST actions are protected with `@Html.AntiForgeryToken()`.

---

## Conclusion
The Vanguard Engine now supports full-cycle incident tracking and resolution, greatly increasing operational transparency and accountability for both our tactical staff and our VIP clientele. All systems are compiling cleanly with zero warnings or errors.
