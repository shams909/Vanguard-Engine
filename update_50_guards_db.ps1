$endpoint = "https://fra.cloud.appwrite.io/v1"
$projectId = "6a018c59002ede066bcc"
$apiKey = "standard_f7a6487acba2727507ae58d778536429900e1455e2a4fa88ffadd439eebf5d28b448c187277b6546bbb1fc8114b66f4dc308f6007805aef815e7bc49d654d7aa76a0b1aa572aeb7391a364b1c427b77191d91bfb1f2b5ce0ef83356be1235d785d9612b0048de8719305f0a42be1efb80baab1ff4cc89b5d5001d4086c84f33b"
$databaseId = "6a018d47000efaba5068"
$collectionId = "users"
$roleId = "6a0242b00027536dcbd2"

$headers = @{
    "X-Appwrite-Project" = $projectId
    "X-Appwrite-Key" = $apiKey
    "Content-Type" = "application/json"
}

$addresses = @(
    "Gulshan 1, Dhaka", "Banani, Dhaka", "Dhanmondi, Dhaka", "Mirpur 10, Dhaka", "Uttara Sector 4, Dhaka",
    "Mohakhali, Dhaka", "Badda, Dhaka", "Malibagh, Dhaka", "Motijheel, Dhaka", "Farmgate, Dhaka",
    "Agargaon, Dhaka", "Mohammadpur, Dhaka", "Khilgaon, Dhaka", "Rampura, Dhaka", "Shahbagh, Dhaka",
    "Jatrabari, Dhaka", "Puran Dhaka", "Savar, Dhaka", "Tongi, Gazipur", "Gazipur City",
    "Narayanaganj City", "Agrabad, Chittagong", "Nasirabad, Chittagong", "GEC Circle, Chittagong", "Halishahar, Chittagong",
    "Kotwali, Chittagong", "Zindabazar, Sylhet", "Uposhohor, Sylhet", "Amberkhana, Sylhet", "Shibganj, Sylhet",
    "Khulna City", "Sonadanga, Khulna", "Boyra, Khulna", "Rajshahi City", "Shaheb Bazar, Rajshahi",
    "Motihar, Rajshahi", "Barisal City", "Sadar Road, Barisal", "Comilla City", "Kandirpar, Comilla",
    "Bogra City", "Mymensingh City", "Faridpur City", "Jessore City", "Dinajpur City",
    "Pabna City", "Kushtia City", "Tangail City", "Nawabganj City", "Cox's Bazar"
)

Write-Host "Fetching users..."
$response = Invoke-RestMethod -Uri "$endpoint/databases/$databaseId/collections/$collectionId/documents?limit=100" -Method Get -Headers $headers

$documents = $response.documents
Write-Host "Found $($documents.Length) users."

$addressIndex = 0

foreach ($doc in $documents) {
    if ($doc.email -match "^guard\d+@gmail\.com$") {
        $docId = $doc.'$id'
        $address = $addresses[$addressIndex % $addresses.Length]
        $addressIndex++
        
        $body = @{
            data = @{
                roleId = $roleId
                address = $address
            }
        } | ConvertTo-Json -Depth 5

        try {
            Invoke-RestMethod -Uri "$endpoint/databases/$databaseId/collections/$collectionId/documents/$docId" -Method Patch -Headers $headers -Body $body
            Write-Host "Updated $($doc.email) with RoleID and Address: $address"
        } catch {
            Write-Host "Error updating $($doc.email): $_"
        }
    }
}

Write-Host "Completed updating guards."
