# Convert logo.png to logo.ico for use as application icon
Add-Type -AssemblyName System.Drawing

$pngPath = Join-Path $PSScriptRoot "logo.png"
$icoPath = Join-Path $PSScriptRoot "logo.ico"

if (Test-Path $pngPath) {
    Write-Host "Converting logo.png to logo.ico..."

    # Load the PNG image
    $img = [System.Drawing.Image]::FromFile($pngPath)

    # Create multiple sizes for the icon (Windows standard sizes)
    $sizes = @(16, 32, 48, 64, 128, 256)

    # Create a temporary bitmap for each size
    $icons = @()

    foreach ($size in $sizes) {
        $bitmap = New-Object System.Drawing.Bitmap($size, $size)
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.DrawImage($img, 0, 0, $size, $size)

        # Save to a memory stream
        $ms = New-Object System.IO.MemoryStream
        $bitmap.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
        $icons += @{Size = $size; Stream = $ms}

        $graphics.Dispose()
    }

    # For simplicity, just use the PNG directly converted to ICO
    # Note: This creates a basic ICO file
    $bitmap256 = New-Object System.Drawing.Bitmap(256, 256)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap256)
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.DrawImage($img, 0, 0, 256, 256)

    # Convert to icon
    $icon = [System.Drawing.Icon]::FromHandle($bitmap256.GetHicon())
    $fileStream = [System.IO.File]::OpenWrite($icoPath)
    $icon.Save($fileStream)
    $fileStream.Close()

    $graphics.Dispose()
    $bitmap256.Dispose()
    $img.Dispose()

    # Clean up streams
    foreach ($iconData in $icons) {
        $iconData.Stream.Dispose()
    }

    Write-Host "Successfully created logo.ico"
    Write-Host "Icon saved to: $icoPath"
} else {
    Write-Host "Error: logo.png not found at $pngPath"
    exit 1
}
