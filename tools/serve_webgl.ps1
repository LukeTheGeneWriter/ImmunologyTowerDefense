# Zero-dependency local static file server for testing the WebGL build.
# Needed for two reasons: (1) file:// URLs can't load Unity's WebGL loader
# (it uses fetch(), which browsers block under file://), and (2) Unity's
# WebGL output ships gzip-compressed files that need a Content-Encoding
# header to be served correctly -- most simple servers, including
# `python -m http.server`, don't set this automatically either.

param(
    [string]$Root = (Join-Path (Split-Path -Parent $PSScriptRoot) "game\Builds\WebGL"),
    [int]$Port = 8000
)

if (-not (Test-Path $Root)) {
    Write-Host "Build folder not found at $Root. Build WebGL first."
    exit 1
}

$listener = New-Object System.Net.HttpListener
$listener.Prefixes.Add("http://localhost:$Port/")
$listener.Start()
Write-Host "Serving $Root at http://localhost:$Port/  (Ctrl+C to stop)"

$mimeMap = @{
    ".html" = "text/html"
    ".js"   = "application/javascript"
    ".css"  = "text/css"
    ".wasm" = "application/wasm"
    ".png"  = "image/png"
    ".ico"  = "image/x-icon"
    ".json" = "application/json"
    ".data" = "application/octet-stream"
}

try {
    while ($listener.IsListening) {
        $context = $listener.GetContext()
        $request = $context.Request
        $response = $context.Response

        $relPath = $request.Url.LocalPath.TrimStart('/')
        if ([string]::IsNullOrEmpty($relPath)) { $relPath = "index.html" }
        $filePath = Join-Path $Root $relPath

        if (-not (Test-Path $filePath -PathType Leaf)) {
            $response.StatusCode = 404
            $response.Close()
            continue
        }

        $isGzip = $filePath.EndsWith(".gz")
        $underlyingExt = if ($isGzip) {
            [System.IO.Path]::GetExtension($filePath.Substring(0, $filePath.Length - 3))
        } else {
            [System.IO.Path]::GetExtension($filePath)
        }

        $contentType = $mimeMap[$underlyingExt]
        if (-not $contentType) { $contentType = "application/octet-stream" }

        $response.ContentType = $contentType
        if ($isGzip) { $response.Headers.Add("Content-Encoding", "gzip") }

        $bytes = [System.IO.File]::ReadAllBytes($filePath)
        $response.ContentLength64 = $bytes.Length
        $response.OutputStream.Write($bytes, 0, $bytes.Length)
        $response.Close()
    }
} finally {
    $listener.Stop()
}
