using Appwrite;
using Appwrite.Services;
using Microsoft.AspNetCore.Identity;

var client = new Client()
    .SetEndpoint("https://fra.cloud.appwrite.io/v1")
    .SetProject("6a018c59002ede066bcc")
    .SetKey("standard_f7a6487acba2727507ae58d778536429900e1455e2a4fa88ffadd439eebf5d28b448c187277b6546bbb1fc8114b66f4dc308f6007805aef815e7bc49d654d7aa76a0b1aa572aeb7391a364b1c427b77191d91bfb1f2b5ce0ef83356be1235d785d9612b0048de8719305f0a42be1efb80baab1ff4cc89b5d5001d4086c84f33b");

var databases = new Databases(client);
string dbId = "6a018d47000efaba5068";
var hasher = new PasswordHasher<object>();

var roleIds = new Dictionary<string, string>
{
    { "Guard",      "6a0242b00027536dcbd2" },
    { "Client",     "6a0243f30039023cbdb0" },
    { "VIPClient",  "6a304e6b499d89ca031c" },
    { "Recruiter",  "6a4161b50001b464c366" },
};

var testUsers = new (string Id, string Username, string Email, string Password, string Role, string Phone)[]
{
    ("e2e_guard01", "e2e_guard", "e2e_guard@vanguard.com", "Test@123456", "Guard", "03001111111"),
    ("e2e_client1", "e2e_client", "e2e_client@vanguard.com", "Test@123456", "Client", "03002222222"),
    ("e2e_vip0001", "e2e_vip", "e2e_vip@vanguard.com", "Test@123456", "VIPClient", "03003333333"),
    ("e2e_recruit", "e2e_recruiter", "e2e_recruiter@vanguard.com", "Test@123456", "Recruiter", "03004444444"),
};

foreach (var (id, username, email, password, role, phone) in testUsers)
{
    string hash = hasher.HashPassword(new object(), password);
    try
    {
        await databases.CreateDocument(dbId, "users", id, new Dictionary<string, object>
        {
            { "username", username },
            { "email", email },
            { "passwordHash", hash },
            { "roleId", roleIds[role] },
            { "isEmailVerified", true },
            { "phoneNumber", phone },
            { "lastLogin", DateTime.UtcNow.ToString("o") },
            { "guardStatus", role == "Guard" ? "Available" : "" }
        });
        Console.WriteLine($"Created {role}: {email}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Skip {email}: {ex.Message}");
    }
}
