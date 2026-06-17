$projectId  = "6a018c59002ede066bcc"
$databaseId = "6a018d47000efaba5068"
$apiKey     = "standard_f7a6487acba2727507ae58d778536429900e1455e2a4fa88ffadd439eebf5d28b448c187277b6546bbb1fc8114b66f4dc308f6007805aef815e7bc49d654d7aa76a0b1aa572aeb7391a364b1c427b77191d91bfb1f2b5ce0ef83356be1235d785d9612b0048de8719305f0a42be1efb80baab1ff4cc89b5d5001d4086c84f33b"
$base       = "https://fra.cloud.appwrite.io/v1"
$headers    = @{
    "X-Appwrite-Project" = $projectId
    "X-Appwrite-Key"     = $apiKey
    "Content-Type"       = "application/json"
}

function Invoke-AW($method, $path, $body = $null) {
    $uri = "$base$path"
    if ($body) {
        return Invoke-RestMethod -Uri $uri -Method $method -Headers $headers -Body ($body | ConvertTo-Json -Depth 5)
    }
    return Invoke-RestMethod -Uri $uri -Method $method -Headers $headers
}

# 1. Create collection
Write-Host "Creating collection..."
try {
    $col = Invoke-AW "Post" "/databases/$databaseId/collections" @{
        collectionId     = "notifications"
        name             = "notifications"
        documentSecurity = $false
    }
    Write-Host "Collection created: $($col.'$id')"
} catch {
    Write-Host "Collection creation response: $_"
}

# 2. Add attributes
Write-Host "Adding attributes..."

# userId
try { Invoke-AW "Post" "/databases/$databaseId/collections/notifications/attributes/string" @{ key = "userId"; size = 255; required = $true } | Out-Null; Write-Host "userId OK" } catch { Write-Host "userId: $_" }

# title
try { Invoke-AW "Post" "/databases/$databaseId/collections/notifications/attributes/string" @{ key = "title"; size = 500; required = $true } | Out-Null; Write-Host "title OK" } catch { Write-Host "title: $_" }

# message
try { Invoke-AW "Post" "/databases/$databaseId/collections/notifications/attributes/string" @{ key = "message"; size = 2000; required = $true } | Out-Null; Write-Host "message OK" } catch { Write-Host "message: $_" }

# type
try { Invoke-AW "Post" "/databases/$databaseId/collections/notifications/attributes/string" @{ key = "type"; size = 50; required = $false; default = "Info" } | Out-Null; Write-Host "type OK" } catch { Write-Host "type: $_" }

# isRead
try { Invoke-AW "Post" "/databases/$databaseId/collections/notifications/attributes/boolean" @{ key = "isRead"; required = $false; default = $false } | Out-Null; Write-Host "isRead OK" } catch { Write-Host "isRead: $_" }

# expiration (datetime)
try { Invoke-AW "Post" "/databases/$databaseId/collections/notifications/attributes/datetime" @{ key = "expiration"; required = $false } | Out-Null; Write-Host "expiration OK" } catch { Write-Host "expiration: $_" }

# 3. Wait for attributes to process then create index
Write-Host "Waiting 5s for attributes to be ready..."
Start-Sleep 5

# userId index
Write-Host "Creating index..."
try {
    Invoke-AW "Post" "/databases/$databaseId/collections/notifications/indexes" @{
        key        = "userId_index"
        type       = "key"
        attributes = @("userId")
        orders     = @("DESC")
    } | Out-Null
    Write-Host "Index created OK"
} catch {
    Write-Host "Index: $_"
}

Write-Host "Done!"
