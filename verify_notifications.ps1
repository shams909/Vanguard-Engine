$projectId  = "6a018c59002ede066bcc"
$databaseId = "6a018d47000efaba5068"
$apiKey     = "standard_f7a6487acba2727507ae58d778536429900e1455e2a4fa88ffadd439eebf5d28b448c187277b6546bbb1fc8114b66f4dc308f6007805aef815e7bc49d654d7aa76a0b1aa572aeb7391a364b1c427b77191d91bfb1f2b5ce0ef83356be1235d785d9612b0048de8719305f0a42be1efb80baab1ff4cc89b5d5001d4086c84f33b"
$base       = "https://fra.cloud.appwrite.io/v1"
$headers    = @{
    "X-Appwrite-Project" = $projectId
    "X-Appwrite-Key"     = $apiKey
}

Write-Host "=== Checking notifications collection ==="
try {
    $col = Invoke-RestMethod -Uri "$base/databases/$databaseId/collections/notifications" -Headers $headers
    Write-Host "Collection OK: $($col.name) (id=$($col.'$id'))"
} catch {
    Write-Host "Collection ERROR: $_"
    exit 1
}

Write-Host ""
Write-Host "=== Attributes ==="
try {
    $attrs = Invoke-RestMethod -Uri "$base/databases/$databaseId/collections/notifications/attributes" -Headers $headers
    foreach ($a in $attrs.attributes) {
        Write-Host "  $($a.key) [$($a.type)] required=$($a.required) status=$($a.status)"
    }
} catch {
    Write-Host "Attributes ERROR: $_"
}

Write-Host ""
Write-Host "=== Indexes ==="
try {
    $idx = Invoke-RestMethod -Uri "$base/databases/$databaseId/collections/notifications/indexes" -Headers $headers
    foreach ($i in $idx.indexes) {
        Write-Host "  $($i.key) [$($i.type)] status=$($i.status)"
    }
} catch {
    Write-Host "Indexes ERROR: $_"
}

Write-Host ""
Write-Host "=== Test: Insert a document ==="
try {
    $body = @{
        userId     = "test_user"
        title      = "Test Notification"
        message    = "This is a test from the provisioning script."
        type       = "Info"
        isRead     = $false
        expiration = "2026-07-17T00:00:00.000Z"
    } | ConvertTo-Json
    $hdrs = $headers + @{ "Content-Type" = "application/json" }
    $doc = Invoke-RestMethod -Uri "$base/databases/$databaseId/collections/notifications/documents" -Method Post -Headers $hdrs -Body $body -ContentType "application/json"
    Write-Host "Insert OK: doc id = $($doc.'$id')"
    
    # Clean up test doc
    Invoke-RestMethod -Uri "$base/databases/$databaseId/collections/notifications/documents/$($doc.'$id')" -Method Delete -Headers $headers | Out-Null
    Write-Host "Cleanup OK"
} catch {
    Write-Host "Insert ERROR: $_"
}

Write-Host ""
Write-Host "=== All checks complete ==="
