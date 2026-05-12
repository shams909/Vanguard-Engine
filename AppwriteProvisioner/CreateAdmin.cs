using Appwrite;
using Appwrite.Services;
using Appwrite.Models;
using Microsoft.AspNetCore.Identity;

// Configuration
string endpoint = "https://fra.cloud.appwrite.io/v1";
string projectId = "6a018c59002ede066bcc";
string apiKey = "standard_f7a6487acba2727507ae58d778536429900e1455e2a4fa88ffadd439eebf5d28b448c187277b6546bbb1fc8114b66f4dc308f6007805aef815e7bc49d654d7aa76a0b1aa572aeb7391a364b1c427b77191d91bfb1f2b5ce0ef83356be1235d785d9612b0048de8719305f0a42be1efb80baab1ff4cc89b5d5001d4086c84f33b";
string databaseId = "6a018d47000efaba5068";
string adminRoleId = "6a0243f200295575e1fe"; // Verified Admin Role ID

// User Details
string newUsername = "musa_manager";
string newEmail = "manager@vanguard.com";
string plainPassword = "AdminPassword123!";

var client = new Client()
    .SetEndpoint(endpoint)
    .SetProject(projectId)
    .SetKey(apiKey);

var databases = new Databases(client);

var hasher = new PasswordHasher<object>();
string hashedPassword = hasher.HashPassword(new object(), plainPassword);

var newUser = new {
    username = newUsername,
    email = newEmail,
    passwordHash = hashedPassword,
    roleId = adminRoleId,
    lastLogin = DateTime.UtcNow
};

try 
{
    Console.WriteLine($"Creating Admin User: {newEmail}...");
    var result = await databases.CreateDocument(databaseId, "users", ID.Unique(), newUser);
    Console.WriteLine("SUCCESS!");
    Console.WriteLine($"ID: {result.Id}");
    Console.WriteLine($"Username: {newUsername}");
    Console.WriteLine($"Password: {plainPassword}");
}
catch (Exception ex)
{
    Console.WriteLine($"FAILED: {ex.Message}");
}
