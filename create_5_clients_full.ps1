$endpoint = "https://fra.cloud.appwrite.io/v1"
$projectId = "6a018c59002ede066bcc"
$apiKey = "standard_f7a6487acba2727507ae58d778536429900e1455e2a4fa88ffadd439eebf5d28b448c187277b6546bbb1fc8114b66f4dc308f6007805aef815e7bc49d654d7aa76a0b1aa572aeb7391a364b1c427b77191d91bfb1f2b5ce0ef83356be1235d785d9612b0048de8719305f0a42be1efb80baab1ff4cc89b5d5001d4086c84f33b"
$databaseId = "6a018d47000efaba5068"
$collectionId = "users"
$clientRoleId = "6a0243f30039023cbdb0"

$headers = @{
    "X-Appwrite-Project" = $projectId
    "X-Appwrite-Key" = $apiKey
    "Content-Type" = "application/json"
}

$names = @("Apex Innovations", "Globex Corporation", "Wayne Enterprises", "Stark Industries", "CyberDyne Systems")
$addresses = @("Dhanmondi, Dhaka", "Gulshan, Dhaka", "Banani, Dhaka", "Motijheel, Dhaka", "Uttara, Dhaka")

Write-Host "Starting to create 5 Clients..."

for ($i = 1; $i -le 5; $i++) {
    $email = "client$i@gmail.com"
    $password = "guard1234" # Using same password to utilize the same known working hash
    $name = $names[$i - 1]
    $address = $addresses[$i - 1]
    $phoneNumber = "01711" + (Get-Random -Minimum 100000 -Maximum 999999)

    $body = @{
        userId = "unique()"
        email = $email
        password = $password
        name = $name
    } | ConvertTo-Json

    try {
        # 1. Create user in Auth
        $response = Invoke-RestMethod -Uri "$endpoint/users" -Method Post -Headers $headers -Body $body
        $userId = $response.'$id'
        
        Write-Host "Created Client ${i} in Auth: $name ($email) - ID: $userId"
        
        # 2. Set email verification to true
        $verificationBody = @{
            emailVerification = $true
        } | ConvertTo-Json
        
        Invoke-RestMethod -Uri "$endpoint/users/$userId/verification" -Method Patch -Headers $headers -Body $verificationBody | Out-Null
        
        # 3. Insert into Database Collection
        $dbBody = @{
            documentId = $userId # Use Auth ID for consistency
            data = @{
                username = $name
                email = $email
                passwordHash = "AQAAAAIAAYagAAAAEBJ0Ve0xIIs8zp/806fFGQKFn29idOFA30Jsm5s9FoxL2gMHbEazhciP2mjvS+ufFQ==" # Hash for guard1234
                isEmailVerified = $true
                roleId = $clientRoleId
                address = $address
                phoneNumber = $phoneNumber
            }
        } | ConvertTo-Json -Depth 5

        $dbResponse = Invoke-RestMethod -Uri "$endpoint/databases/$databaseId/collections/$collectionId/documents" -Method Post -Headers $headers -Body $dbBody
        Write-Host "  -> Inserted into DB successfully. Role: Client"
        
    } catch {
        Write-Host "Error processing Client $i ($email): $_"
    }
}

Write-Host "Completed creating 5 Clients. Password for all is: guard1234"
