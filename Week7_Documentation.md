# Week 7: Guard Ratings & Admin Analytics

## Overview
Week 7 introduces a powerful macro-level intelligence and feedback loop to the Vanguard Engine. By implementing a robust Guard Rating system and a dynamic, Chart.js-powered Admin Analytics dashboard, we have bridged the gap between client satisfaction and high-level operational oversight.

## Key Features Implemented

### 1. Client Feedback & Guard Ratings
We successfully deployed a specialized feedback pipeline allowing clients to evaluate the tactical performance of deployed guards.
- **Service Evaluation UI:** A new widget on the Client Dashboard securely redirects to the Rating Submission interface.
- **Star-Rating Matrix:** Clients can evaluate guards using an intuitive 1-5 scale (ranging from *Poor* to *Elite*), along with a required detailed comments section.
- **Dynamic Guard Roster:** The evaluation form automatically cross-references the active Vanguard database, pulling in valid security personnel for assessment.

### 2. Admin Analytics Dashboard
A brand new Intelligence & Metrics dashboard has been built for the Admin Operations Center to provide real-time strategic data.
- **Macro KPIs:** Four high-visibility data cards track Total Active Guards, Total Client Requests, System-Wide Average Guard Rating, and Total Logged Incidents.
- **Client Deployment Bar Chart:** Visualizes real-time deployment velocity, separating requests by `Pending`, `Active`, and `Completed` statuses.
- **Guard Performance Distribution:** A horizontal bar chart stacking up the raw counts of 1-Star to 5-Star evaluations to quickly spot workforce quality trends.
- **Incident Resolution Matrix:** A dual-color doughnut chart plotting the ratio of `Open` vs `Resolved` security incidents.

### 3. High-Contrast Chart Optimization
- Modified the default `Chart.js` rendering engine to use a bold, high-contrast dark brown (`#4a2c11`) with a `600` font weight, ensuring maximum readability against both light and dark dashboard backgrounds.

---

## Technical Architecture

### Appwrite Database Schema
A new collection was provisioned using the `AppwriteProvisioner`:
- **Collection Name:** `ratings`
- **Attributes:**
  - `clientId` (string, 255): Links the evaluation to the submitting client.
  - `guardId` (string, 255): Identifies the specific guard being reviewed.
  - `guardName` (string, 255): Cached display name of the evaluated guard.
  - `score` (integer, max 5, min 1): The quantitative performance metric.
  - `comments` (string, 5000): Qualitative feedback and operational notes.
  - `shiftId` (string, 255, optional): Hooks the rating to a specific deployment shift.

### Backend Infrastructure
- **`Rating.cs` (Entity):** C# Model mapping to the Appwrite NoSQL schema using `[JsonProperty]`.
- **`IRatingRepository` / `AppwriteRatingRepository`:** Built to handle secure CRUD operations and calculate average scores via targeted Appwrite queries.
- **`AnalyticsController.cs`:** A powerful aggregation endpoint that seamlessly combines data from `IUserService`, `IClientRequestService`, `IIncidentService`, and `IRatingService` to serialize complex metrics into simple `ViewBag` arrays.
- **`RatingController.cs`:** Manages the secure submission of client evaluations.

### Security & Validation
- **Role-Based Rendering:** The Analytics dashboard is strictly guarded by `[Authorize(Roles = "Admin")]`.
- **Input Sanitization:** Rating scopes are hard-capped between 1 and 5 at both the UI and Controller levels.
- **Anti-Forgery Measures:** All rating submissions are protected with ASP.NET Core's `@Html.AntiForgeryToken()`.

---

## Conclusion
The Vanguard Engine now boasts a comprehensive feedback loop and a visually striking macro-analytics hub. The system operates seamlessly, passing all `dotnet build` constraints with **0 warnings and 0 errors**. Vanguard Administrators can now track operational health at a glance.
