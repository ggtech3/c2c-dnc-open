param([string]$OutputDirectory = (Join-Path $PSScriptRoot '..\Assets'))

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$resolvedOutput = [IO.Path]::GetFullPath($OutputDirectory)
[IO.Directory]::CreateDirectory($resolvedOutput) | Out-Null

function New-LogoBitmap([int]$size, [Drawing.Color]$ink) {
    $bitmap = [Drawing.Bitmap]::new($size, $size, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.Clear([Drawing.Color]::Transparent)
    $scale = $size / 256.0
    $circlePen = [Drawing.Pen]::new($ink, [Math]::Max(2, 15 * $scale))
    $circlePen.Alignment = [Drawing.Drawing2D.PenAlignment]::Inset
    $markPen = [Drawing.Pen]::new($ink, [Math]::Max(2, 20 * $scale))
    $markPen.StartCap = [Drawing.Drawing2D.LineCap]::Square
    $markPen.EndCap = [Drawing.Drawing2D.LineCap]::Square
    $markPen.LineJoin = [Drawing.Drawing2D.LineJoin]::Miter
    $graphics.DrawEllipse($circlePen, 17 * $scale, 17 * $scale, 222 * $scale, 222 * $scale)
    $graphics.DrawLines($markPen, [Drawing.PointF[]]@(
        [Drawing.PointF]::new(94*$scale,78*$scale),
        [Drawing.PointF]::new(178*$scale,78*$scale),
        [Drawing.PointF]::new(178*$scale,154*$scale),
        [Drawing.PointF]::new(132*$scale,154*$scale)))
    $graphics.DrawLines($markPen, [Drawing.PointF[]]@(
        [Drawing.PointF]::new(132*$scale,111*$scale),
        [Drawing.PointF]::new(83*$scale,111*$scale),
        [Drawing.PointF]::new(83*$scale,181*$scale),
        [Drawing.PointF]::new(173*$scale,181*$scale)))
    $circlePen.Dispose(); $markPen.Dispose(); $graphics.Dispose()
    return $bitmap
}

function Get-PngBytes([Drawing.Bitmap]$bitmap) {
    $stream = [IO.MemoryStream]::new()
    $bitmap.Save($stream, [Drawing.Imaging.ImageFormat]::Png)
    $bytes = $stream.ToArray()
    $stream.Dispose()
    return $bytes
}

function New-AppIconBitmap([int]$size) {
    $bitmap = [Drawing.Bitmap]::new($size, $size, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.Clear([Drawing.Color]::Transparent)
    $scale = $size / 256.0
    $blue = [Drawing.Color]::FromArgb(255, 25, 92, 180)
    $white = [Drawing.Color]::White
    $graphics.FillEllipse([Drawing.SolidBrush]::new($blue), 5*$scale, 5*$scale, 246*$scale, 246*$scale)
    $circlePen = [Drawing.Pen]::new($white, [Math]::Max(2, 10*$scale))
    $circlePen.Alignment = [Drawing.Drawing2D.PenAlignment]::Inset
    $markPen = [Drawing.Pen]::new($white, [Math]::Max(2, 24*$scale))
    $markPen.StartCap = [Drawing.Drawing2D.LineCap]::Square
    $markPen.EndCap = [Drawing.Drawing2D.LineCap]::Square
    $markPen.LineJoin = [Drawing.Drawing2D.LineJoin]::Miter
    $graphics.DrawEllipse($circlePen, 18*$scale, 18*$scale, 220*$scale, 220*$scale)
    $graphics.DrawLines($markPen, [Drawing.PointF[]]@(
        [Drawing.PointF]::new(90*$scale,73*$scale), [Drawing.PointF]::new(181*$scale,73*$scale),
        [Drawing.PointF]::new(181*$scale,155*$scale), [Drawing.PointF]::new(130*$scale,155*$scale)))
    $graphics.DrawLines($markPen, [Drawing.PointF[]]@(
        [Drawing.PointF]::new(130*$scale,108*$scale), [Drawing.PointF]::new(78*$scale,108*$scale),
        [Drawing.PointF]::new(78*$scale,185*$scale), [Drawing.PointF]::new(176*$scale,185*$scale)))
    $circlePen.Dispose(); $markPen.Dispose(); $graphics.Dispose()
    return $bitmap
}

$darkInk = [Drawing.Color]::FromArgb(255, 23, 39, 61)
$lightInk = [Drawing.Color]::FromArgb(255, 240, 246, 252)
$darkBitmap = New-LogoBitmap 256 $darkInk
$lightBitmap = New-LogoBitmap 256 $lightInk
$darkBitmap.Save((Join-Path $resolvedOutput 'C2CDnc-logo-dark.png'), [Drawing.Imaging.ImageFormat]::Png)
$lightBitmap.Save((Join-Path $resolvedOutput 'C2CDnc-logo-light.png'), [Drawing.Imaging.ImageFormat]::Png)
$darkBitmap.Dispose(); $lightBitmap.Dispose()

$sizes = @(16, 24, 32, 48, 64, 128, 256)
$images = foreach ($size in $sizes) {
    $bitmap = New-AppIconBitmap $size
    $bytes = Get-PngBytes $bitmap
    $bitmap.Dispose()
    [pscustomobject]@{ Size = $size; Bytes = $bytes }
}

$appPreview = New-AppIconBitmap 256
$appPreview.Save((Join-Path $resolvedOutput 'C2CDnc-app-icon.png'), [Drawing.Imaging.ImageFormat]::Png)
$appPreview.Dispose()

$iconPath = Join-Path $resolvedOutput 'C2CDnc.ico'
$stream = [IO.File]::Create($iconPath)
$writer = [IO.BinaryWriter]::new($stream)
$writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]$images.Count)
$offset = 6 + (16 * $images.Count)
foreach ($image in $images) {
    $dimension = if ($image.Size -eq 256) { [byte]0 } else { [byte]$image.Size }
    $writer.Write($dimension); $writer.Write($dimension)
    $writer.Write([byte]0); $writer.Write([byte]0)
    $writer.Write([uint16]1); $writer.Write([uint16]32)
    $writer.Write([uint32]$image.Bytes.Length); $writer.Write([uint32]$offset)
    $offset += $image.Bytes.Length
}
foreach ($image in $images) { $writer.Write([byte[]]$image.Bytes) }
$writer.Dispose(); $stream.Dispose()

Write-Host "Generated $iconPath"
