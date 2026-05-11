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
    var rolesList = await databases.ListDocuments(databaseId, "roles", queries: new List<string> { Query.Equal("roleName", "Admin") });
    if (rolesList.Total > 0)
    {
        string adminRoleId = rolesList.Documents[0].Id;
        Console.WriteLine($"ADMIN_ROLE_ID:{adminRoleId}");
        
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
    }
    else
    {
        Console.WriteLine("Admin role not found.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
