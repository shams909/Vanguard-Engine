$endpoint = "https://fra.cloud.appwrite.io/v1"
$projectId = "6a018c59002ede066bcc"
$apiKey = "standard_f7a6487acba2727507ae58d778536429900e1455e2a4fa88ffadd439eebf5d28b448c187277b6546bbb1fc8114b66f4dc308f6007805aef815e7bc49d654d7aa76a0b1aa572aeb7391a364b1c427b77191d91bfb1f2b5ce0ef83356be1235d785d9612b0048de8719305f0a42be1efb80baab1ff4cc89b5d5001d4086c84f33b"
$databaseId = "6a018d47000efaba5068"
$collectionId = "users"

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

Write-Host "Starting to insert 50 guards into Database Collection..."

for ($i = 1; $i -le 50; $i++) {
    $email = "guard$i@gmail.com"
    $baseName = $names[($i - 1) % $names.Length]
    $username = "$baseName$i"
    
    $body = @{
        documentId = "unique()"
        data = @{
            username = $username
            email = $email
            passwordHash = "guard1234"
            isEmailVerified = $true
            guardStatus = "Available"
        }
    } | ConvertTo-Json -Depth 5

    try {
        $response = Invoke-RestMethod -Uri "$endpoint/databases/$databaseId/collections/$collectionId/documents" -Method Post -Headers $headers -Body $body
        $docId = $response.'$id'
        Write-Host "Inserted guard ${i} into DB: $username ($email) - DocID: $docId"
    } catch {
        Write-Host "Error inserting guard ${i} into DB ($email): $_"
    }
}

Write-Host "Completed inserting 50 guards into Database Collection."
