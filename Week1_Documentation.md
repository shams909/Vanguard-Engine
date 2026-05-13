# Vanguard Engine - Week 1 Technical Master Document

This document is the **ultimate technical breakdown** of everything accomplished in Week 1. It is designed to be a foolproof guide so you can answer exactly *where* any piece of code lives, *how* it works from A to Z, and *why* it was built that way.

---

## 1. Quick Cheat Sheet: "Where is the logic for X?"
If your professor asks "Where does X happen?", use this cheat sheet to point to the exact folder and file:

*   **"Where is the UI / HTML?"** 👉 **`Views/`** folder. 
    *   *Example*: The layout and background color is in `Views/Shared/_PremiumLayout.cshtml`.
*   **"Where does the form data go when I click Submit?"** 👉 **`Controllers/`** folder.
    *   *Example*: Clicking "Submit Application" goes to `Controllers/GuardController.cs` (specifically the `[HttpPost] Apply` method).
*   **"Where does the database connection happen?"** 👉 **`Services/`** folder.
    *   *Example*: Appwrite configuration is in `AppwriteService.cs`. The actual insert/update to the database happens in `GuardApplicationService.cs`.
*   **"Where do you define what fields a Guard has?"** 👉 **`Entities/`** and **`Models/`**.
    *   *Example*: `Entities/GuardApplication.cs` is the exact match to the database. `Models/GuardApplyViewModel.cs` is the exact match to the frontend form.
*   **"How do I know who is logged in?"** 👉 It's stored in a secure Cookie. We access it using `User.Identity.Name` in the View, or `GetUserId()` in the Controller.

---

## 2. Deep Dive: A to Z Flow - The Guard Recruitment Pipeline
If the professor asks "Walk me through how a Guard applies and gets approved," here is the exact step-by-step technical flow.

### Step 1: The Guard Sees the Application Form
1.  **The Request**: The Guard clicks the "Apply" link in the navbar.
2.  **The Controller**: The request hits `Controllers/GuardController.cs` at the `[HttpGet] Apply()` method.
3.  **The View**: The controller returns `Views/Guard/Apply.cshtml`. This file contains standard HTML mixed with C# (Razor) and uses the `GuardApplyViewModel.cs` to bind the form fields securely.

### Step 2: The Guard Submits the Form
1.  **The POST**: The Guard fills out the form and clicks Submit. This sends an HTTP POST request.
2.  **The Controller**: The request hits `Controllers/GuardController.cs` at the `[HttpPost] Apply(GuardApplyViewModel model)` method.
3.  **Validation**: The controller checks `ModelState.IsValid`. If the user forgot a required field (like Phone Number), it instantly stops and returns the view with an error message.
4.  **Data Mapping**: We take the `GuardApplyViewModel` (the form data) and map it into a `GuardApplication` Entity (the database structure). We manually set `FullName = model.FullName.Trim()` to clean up the data.
5.  **Passing to Service**: The Controller calls `await _guardApplicationService.ApplyAsync(GetUserId(), application);`. It passes the currently logged-in user's ID and the new application data.

### Step 3: Saving to the Database (Appwrite)
1.  **The Service**: The logic moves to `Services/GuardApplicationService.cs` inside the `ApplyAsync` method.
2.  **Data Formatting**: The service creates a `Dictionary<string, object>` to format the data exactly how Appwrite wants it. It hardcodes the status: `["Status"] = "Pending"`.
3.  **The API Call**: The service uses the Appwrite SDK (`_databases.CreateDocument`) to insert the data into our cloud NoSQL database. 
4.  **The Return**: If successful, it returns `true` back to the Controller.
5.  **The Redirect**: The Controller sees `true`, sets a Success message in `TempData["SuccessMessage"]`, and redirects the Guard to their `/Guard/MyApplications` dashboard.

### Step 4: The Admin Reviews and Approves
1.  **The Dashboard**: The Admin goes to `/Guard/Applications`. The `GuardController` asks `GuardApplicationService` to fetch ALL applications from the database and passes them to `Views/Guard/Applications.cshtml`.
2.  **The Review Screen**: The Admin clicks "Review" on a specific candidate. This opens `Views/Guard/Review.cshtml` which displays all the Guard's details.
3.  **The Action**: The Admin clicks the green "Approve Candidate" button. This triggers a POST request to `Controllers/GuardController.cs` at `[HttpPost] Approve(string id)`.
4.  **Database Update**: The Controller tells the Service to approve it: `_guardApplicationService.ApproveAsync(id)`.
5.  **Appwrite Execution**: Inside the Service, it calls `_databases.UpdateDocument` and passes a single updated field: `["Status"] = "Approved"`. The database is instantly updated, and the Guard will now see "Approved" on their own screen.

---

## 3. Deep Dive: A to Z Flow - Registration & Authentication
If the professor asks "How is security and login handled?", explain this:

### Registration (Creating the Account)
1.  User fills out `Views/Auth/Register.cshtml` and submits.
2.  Data goes to `AuthController.Register(POST)`.
3.  `AuthController` calls `AuthService.RegisterAsync()`.
4.  Inside `AuthService`, it talks to the **Appwrite Account API** (`_account.Create`) to securely create the user with their Email and Password. Appwrite hashes the password automatically on their secure servers; we NEVER touch raw passwords.
5.  It also creates a matching document in our "Users" database collection to keep track of their Role (e.g., "Admin", "Guard").

### Login (Establishing the Session)
1.  User enters credentials in `Views/Auth/Login.cshtml`.
2.  `AuthController` passes them to `AuthService.LoginAsync()`.
3.  `AuthService` asks Appwrite to verify the Email and Password via `_account.CreateEmailPasswordSession()`. If Appwrite says it's valid, it returns the User ID.
4.  `AuthService` then queries our database to find out what "Role" this User ID has.
5.  **The Magic**: Back in `AuthController`, we take the User ID and Role and create a set of "Claims". We bundle these Claims into a secure, encrypted **Cookie** using ASP.NET Core's `HttpContext.SignInAsync`. 
6.  For every subsequent request (clicking links, refreshing pages), the user's browser sends this Cookie, proving who they are and what role they hold.

---

## 4. Advanced Technical Features to Mention
If you want to impress the professor, mention these specific technical implementations:

*   **Dependency Injection (DI)**: Look inside `Program.cs`. We don't manually create our Services (e.g., `new GuardApplicationService()`). Instead, we register them using `builder.Services.AddScoped<IGuardApplicationService, GuardApplicationService>()`. This tells ASP.NET to automatically inject the service into our Controllers. This is an enterprise best practice for clean, testable code.
*   **Role-Based Authorization**: Look at the top of `GuardController.cs`. We use tags like `[Authorize(Roles = "Admin,Recruiter")]`. This single line of code guarantees that a standard Guard can NEVER access the applications list, even if they guess the URL. ASP.NET automatically intercepts the request and blocks it.
*   **Asynchronous Programming (`async`/`await`)**: Every single database call in our `Services/` folder uses `await`. This means that while our server is waiting for Appwrite to respond across the internet, the thread is freed up to serve other users. This makes Vanguard Engine highly scalable.
*   **CSS Glassmorphism & Lucide Icons**: To avoid a generic look, we built a custom UI in `_PremiumLayout.cshtml`. We used CSS `backdrop-filter: blur(40px) saturate(150%)` to achieve true iOS-style transparency, and we strictly used **Lucide SVG Icons** instead of emojis for scalable, enterprise-grade vector graphics.
