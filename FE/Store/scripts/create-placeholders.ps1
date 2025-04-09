$categories = @(
    "laptop", "gaming", "vr", "keyboard", "chair", "desk", "cabinet", "audio",
    "mouse", "bag", "software", "arm", "phone-stand", "ram", "ssd", "charger"
)

foreach ($category in $categories) {
    Copy-Item -Force ".\public\assets\placeholder.png" ".\public\assets\categories\$category.jpg"
}

Write-Host "Created placeholder images for all categories."
