using Appwrite;
using Appwrite.Services;
using Appwrite.Models;
using Microsoft.AspNetCore.Identity;

var client = new Client()
    .SetEndpoint("https://fra.cloud.appwrite.io/v1")
    .SetProject("6a018c59002ede066bcc")
    .SetKey("standard_f7a6487acba2727507ae58d778536429900e1455e2a4fa88ffadd439eebf5d28b448c187277b6546bbb1fc8114b66f4dc308f6007805aef815e7bc49d654d7aa76a0b1aa572aeb7391a364b1c427b77191d91bfb1f2b5ce0ef83356be1235d785d9612b0048de8719305f0a42be1efb80baab1ff4cc89b5d5001d4086c84f33b");

var databases = new Databases(client);
string databaseId = "6a018d47000efaba5068";

Console.WriteLine("Fetching Admin Role ID...");

try 
{
    // --- Recruitment Setup ---
    Console.WriteLine("--- Setting up Recruitment Collection ---");
    string recruitmentCollId = "hiring_notices";
    try {
        await databases.CreateCollection(databaseId, recruitmentCollId, "Recruitment Notices", permissions: new List<string> { "read(\"any\")", "create(\"users\")", "update(\"users\")", "delete(\"users\")" });
        Console.WriteLine("Collection 'hiring_notices' created.");
    } catch { Console.WriteLine("Collection 'hiring_notices' exists."); }

    string[,] attributes = {
        { "title", "255", "true" },
        { "referenceCode", "100", "true" },
        { "description", "5000", "true" },
        { "requirements", "5000", "true" },
        { "location", "255", "true" },
        { "jobType", "100", "true" },
        { "priority", "50", "true" },
        { "salaryRange", "100", "false" },
        { "status", "50", "true" },
        { "postedByUserId", "255", "true" },
        { "expiryDate", "100", "false" }
    };

    for (int i = 0; i < attributes.GetLength(0); i++) {
        try {
            await databases.CreateStringAttribute(databaseId, recruitmentCollId, attributes[i,0], int.Parse(attributes[i,1]), bool.Parse(attributes[i,2]));
            Console.WriteLine($"- Attribute '{attributes[i,0]}' added.");
        } catch { Console.WriteLine($"- Attribute '{attributes[i,0]}' exists."); }
    }

    // --- Assigned Shifts Setup ---
    Console.WriteLine("\n--- Setting up Assigned Shifts Collection ---");
    string assignedShiftsCollId = "assigned_shifts";
    try {
        await databases.CreateCollection(databaseId, assignedShiftsCollId, "Assigned Shifts", permissions: new List<string> { "read(\"any\")", "create(\"users\")", "update(\"users\")", "delete(\"users\")" });
        Console.WriteLine("Collection 'assigned_shifts' created.");
    } catch { Console.WriteLine("Collection 'assigned_shifts' exists."); }

    string[,] shiftAttributes = {
        { "guardId", "255", "true" },
        { "guardName", "255", "true" },
        { "shiftDate", "100", "true" },
        { "startTime", "50", "true" },
        { "endTime", "50", "true" },
        { "status", "50", "true" }
    };

    for (int i = 0; i < shiftAttributes.GetLength(0); i++) {
        try {
            await databases.CreateStringAttribute(databaseId, assignedShiftsCollId, shiftAttributes[i,0], int.Parse(shiftAttributes[i,1]), bool.Parse(shiftAttributes[i,2]));
            Console.WriteLine($"- Attribute '{shiftAttributes[i,0]}' added.");
        } catch { Console.WriteLine($"- Attribute '{shiftAttributes[i,0]}' exists."); }
    }

    // --- Incidents Setup ---
    Console.WriteLine("\n--- Setting up Incidents Collection ---");
    string incidentsCollId = "incidents";
    try {
        await databases.CreateCollection(databaseId, incidentsCollId, "Incidents and Complaints", permissions: new List<string> { "read(\"any\")", "create(\"users\")", "update(\"users\")", "delete(\"users\")" });
        Console.WriteLine("Collection 'incidents' created.");
    } catch { Console.WriteLine("Collection 'incidents' exists."); }

    string[,] incidentAttributes = {
        { "reportedByUserId", "255", "true" },
        { "reportedByName", "255", "true" },
        { "reporterRole", "50", "true" },
        { "type", "50", "true" },
        { "title", "255", "true" },
        { "description", "5000", "true" },
        { "status", "50", "true" },
        { "resolutionNotes", "5000", "false" },
        { "resolvedByAdminId", "255", "false" }
    };

    for (int i = 0; i < incidentAttributes.GetLength(0); i++) {
        try {
            await databases.CreateStringAttribute(databaseId, incidentsCollId, incidentAttributes[i,0], int.Parse(incidentAttributes[i,1]), bool.Parse(incidentAttributes[i,2]));
            Console.WriteLine($"- Attribute '{incidentAttributes[i,0]}' added.");
        } catch { Console.WriteLine($"- Attribute '{incidentAttributes[i,0]}' exists."); }
    }

    // --- Guard Applications Update ---
    try {
        await databases.CreateStringAttribute(databaseId, "guard_applications", "jobId", 100, false);
        Console.WriteLine("- Attribute 'jobId' added to guard_applications.");
    } catch { }

    // --- Guard Shifts Update ---
    try {
        await databases.CreateStringAttribute(databaseId, "guard_shifts", "assignedShiftId", 100, false);
        Console.WriteLine("- Attribute 'assignedShiftId' added to guard_shifts.");
    } catch { }

    // --- Users Collection Update ---
    Console.WriteLine("\n--- Updating Users Collection Schema ---");
    try {
        await databases.CreateBooleanAttribute(databaseId, "users", "isEmailVerified", false, false);
        Console.WriteLine("- Attribute 'isEmailVerified' added to users.");
    } catch (Exception ex) { Console.WriteLine($"- Attribute 'isEmailVerified' skipped: {ex.Message}"); }

    try {
        await databases.CreateStringAttribute(databaseId, "users", "verificationToken", 255, false);
        Console.WriteLine("- Attribute 'verificationToken' added to users.");
    } catch (Exception ex) { Console.WriteLine($"- Attribute 'verificationToken' skipped: {ex.Message}"); }

    try {
        await databases.CreateDatetimeAttribute(databaseId, "users", "verificationTokenExpiry", false);
        Console.WriteLine("- Attribute 'verificationTokenExpiry' added to users.");
    } catch (Exception ex) { Console.WriteLine($"- Attribute 'verificationTokenExpiry' skipped: {ex.Message}"); }

    try {
        await databases.CreateStringAttribute(databaseId, "users", "resetToken", 255, false);
        Console.WriteLine("- Attribute 'resetToken' added to users.");
    } catch (Exception ex) { Console.WriteLine($"- Attribute 'resetToken' skipped: {ex.Message}"); }

    try {
        await databases.CreateDatetimeAttribute(databaseId, "users", "resetTokenExpiry", false);
        Console.WriteLine("- Attribute 'resetTokenExpiry' added to users.");
    } catch (Exception ex) { Console.WriteLine($"- Attribute 'resetTokenExpiry' skipped: {ex.Message}"); }

    // --- Admin Creation ---
    Console.WriteLine("\nFetching Admin Role ID...");
    var rolesList = await databases.ListDocuments(databaseId, "roles", queries: new List<string> { Query.Equal("roleName", "Admin") });
    if (rolesList.Total > 0)
    {
        string adminRoleId = rolesList.Documents[0].Id;
        Console.WriteLine($"ADMIN_ROLE_ID:{adminRoleId}");
        
        try {
            var hasher = new PasswordHasher<object>();
            string hashedPassword = hasher.HashPassword(new object(), "AdminMusa@123");

            var newUser = new {
                username = "musa_admin",
                email = "musa@vanguard.com",
                passwordHash = hashedPassword,
                roleId = adminRoleId,
                lastLogin = DateTime.UtcNow
            };

            await databases.CreateDocument(databaseId, "users", ID.Unique(), newUser);
            Console.WriteLine("USER_CREATED:musa@vanguard.com / AdminMusa@123");
        } catch (Exception ex) {
            Console.WriteLine($"User creation skipped: {ex.Message}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Fatal Error: {ex.Message}");
}
