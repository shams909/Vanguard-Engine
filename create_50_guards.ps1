$endpoint = "https://fra.cloud.appwrite.io/v1"
$projectId = "6a018c59002ede066bcc"
$apiKey = "standard_f7a6487acba2727507ae58d778536429900e1455e2a4fa88ffadd439eebf5d28b448c187277b6546bbb1fc8114b66f4dc308f6007805aef815e7bc49d654d7aa76a0b1aa572aeb7391a364b1c427b77191d91bfb1f2b5ce0ef83356be1235d785d9612b0048de8719305f0a42be1efb80baab1ff4cc89b5d5001d4086c84f33b"

$headers = @{
    "X-Appwrite-Project" = $projectId
    "X-Appwrite-Key" = $apiKey
    "Content-Type" = "application/json"
}

$names = @(
    "Karim Hasan", "Hashem Mahmud", "Rafiqul Islam", "Tariq Rahman", "Shafiqur Rahman",
    "Zahirul Haque", "Ashraful Alam", "Faisal Ahmed", "Mahbub Hossain", "Jalal Uddin",
    "Habibur Rahman", "Nazmul Huda", "Anisur Rahman", "Sabbir Hossain", "Tofazzal Hossain",
    "Saiful Islam", "Kamrul Hasan", "Farid Uddin", "Mizanur Rahman", "Zillur Rahman",
    "Abul Kalam", "Nurul Islam", "Rezaul Karim", "Enamul Haque", "Shamsul Alam",
    "Badrul Amin", "Zahidul Islam", "Mostafa Kamal", "Tariqul Islam", "Rabiul Awal",
    "Ariful Islam", "Aminul Hoque", "Shariful Islam", "Nazrul Islam", "Moniruzzaman",
    "Ataur Rahman", "Fazlul Haque", "Shahjahan Ali", "Akhtaruzzaman", "Wahidul Islam",
    "Mahfuzur Rahman", "Sirajul Islam", "Khaled Mahmud", "Rakibul Hasan", "Imran Hossain",
    "Ziaur Rahman", "Mahmudul Hasan", "Azizul Haque", "Sadiqur Rahman", "Mokhlesur Rahman"
)

Write-Host "Starting to create 50 guards..."

for ($i = 1; $i -le 50; $i++) {
    $email = "guard$i@gmail.com"
    $password = "guard1234"
    $baseName = $names[($i - 1) % $names.Length]
    $name = "$baseName$i"

    $body = @{
        userId = "unique()"
        email = $email
        password = $password
        name = $name
    } | ConvertTo-Json

    try {
        # Create user
        $response = Invoke-RestMethod -Uri "$endpoint/users" -Method Post -Headers $headers -Body $body
        $userId = $response.'$id'
        
        Write-Host "Created guard ${i}: $name ($email) - ID: $userId"
        
        # Set email verification to true
        $verificationBody = @{
            emailVerification = $true
        } | ConvertTo-Json
        
        $verifyResponse = Invoke-RestMethod -Uri "$endpoint/users/$userId/verification" -Method Patch -Headers $headers -Body $verificationBody
        Write-Host "  -> Verification set to true for $email"
    } catch {
        Write-Host "Error creating guard $i ($email): $_"
    }
}

Write-Host "Completed creating 50 guards."
