Write-Host "Starting ClipHunta Setup for Windows..."

# Function to check if a command exists
function Test-CommandExists {
    param (
        [string]$command
    )
    return (Get-Command $command -ErrorAction SilentlyContinue) -ne $null
}

# Check for .NET 8 SDK
Write-Host "Checking for .NET 8 SDK..."
$dotnetSdkInstalled = $false
if (Test-CommandExists dotnet) {
    $sdkList = dotnet --list-sdks
    if ($sdkList -match "8\.0\.") { # Escaped backslashes
        Write-Host ".NET 8 SDK found."
        $dotnetSdkInstalled = $true
    }
}

if (-not $dotnetSdkInstalled) {
    Write-Warning ".NET 8 SDK not found."
    Write-Host "Please install the .NET 8 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0"
    Write-Host "Then re-run this script."
    # Optionally exit if critical: # exit 1
}

# Check for Python 3
Write-Host "Checking for Python 3..."
$pythonInstalled = $false
if (Test-CommandExists python) {
    $pyVersion = try { python --version 2>&1 } catch { $null }
    if ($pyVersion -match "Python 3\.") { # Escaped backslash
        Write-Host "Python 3 found."
        $pythonInstalled = $true
    }
} elseif (Test-CommandExists python3) {
    $pyVersion = try { python3 --version 2>&1 } catch { $null }
    if ($pyVersion -match "Python 3\.") { # Escaped backslash
        Write-Host "Python 3 found."
        $pythonInstalled = $true
    }
}

if (-not $pythonInstalled) {
    Write-Warning "Python 3 not found."
    Write-Host "Please install Python 3. You can download it from: https://www.python.org/downloads/windows/"
    Write-Host "Alternatively, if you have Chocolatey installed, you can run: choco install python"
    Write-Host "Ensure pip is included with the installation."
    Write-Host "Then re-run this script."
}

# Check for pip (Python package installer)
Write-Host "Checking for pip..."
$pipInstalled = $false
if (Test-CommandExists pip) {
    Write-Host "pip found."
    $pipInstalled = $true
} elseif (Test-CommandExists pip3) {
    Write-Host "pip3 found."
    $pipInstalled = $true
}

if (-not $pipInstalled -and $pythonInstalled) {
    Write-Warning "pip (or pip3) not found, but Python seems installed."
    Write-Host "Please ensure pip is installed and available in your PATH. It's usually included with Python."
    Write-Host "You might need to repair your Python installation or add Python's Scripts directory to PATH."
} elseif (-not $pipInstalled) {
    Write-Warning "pip (or pip3) not found."
}

# Install twitch-dl
Write-Host "Attempting to install/update twitch-dl..."
if ($pipInstalled) {
    $pipCmd = if (Test-CommandExists pip3) { "pip3" } else { "pip" }
    Invoke-Expression "$pipCmd install --user --upgrade twitch-dl"
    Write-Host "If 'twitch-dl' command is not found after installation, you might need to add the Python user scripts directory to your PATH."
    Write-Host "Typically, this is: %APPDATA%\Python\PythonXX\Scripts (replace XX with your Python version e.g., Python39)" # Escaped backslashes
} else {
    Write-Warning "pip not found, cannot install twitch-dl automatically."
}

# Check for Tesseract OCR
Write-Host "Checking for Tesseract OCR..."
$tesseractInstalled = $false
if (Test-CommandExists tesseract) {
    Write-Host "Tesseract OCR found."
    $tesseractInstalled = $true
} else {
    # Check common Chocolatey path
    $chocoTesseractPath = "C:\Program Files\Tesseract-OCR\tesseract.exe" # Escaped backslashes
    if (Test-Path $chocoTesseractPath) {
        Write-Host "Tesseract OCR found at $chocoTesseractPath (likely via Chocolatey)."
        Write-Host "Consider adding 'C:\Program Files\Tesseract-OCR' to your PATH environment variable if not already present." # Escaped backslashes
        $tesseractInstalled = $true
    }
}

if (-not $tesseractInstalled) {
    Write-Warning "Tesseract OCR not found in PATH or common Chocolatey location."
    Write-Host "Please install Tesseract OCR. Recommended method is using Chocolatey: choco install tesseract-ocr"
    Write-Host "Alternatively, download from: https://github.com/UB-Mannheim/tesseract/wiki"
    Write-Host "After installation, ensure Tesseract's directory is added to your PATH."
    Write-Host "Then re-run this script."
}

# Check for Tesseract OCR Data
Write-Host "Checking for Tesseract OCR language data (e.g., eng.traineddata)..."
$tessdataPrefix = [Environment]::GetEnvironmentVariable("TESSDATA_PREFIX", "Machine")
if (-not $tessdataPrefix) {
    $tessdataPrefix = [Environment]::GetEnvironmentVariable("TESSDATA_PREFIX", "User")
}

$tesseractInstallDir = ""
if ($tesseractInstalled) {
    if (Test-CommandExists tesseract) {
        $tesseractLocation = (Get-Command tesseract).Source
        $tesseractInstallDir = (Get-Item $tesseractLocation).DirectoryName
    } elseif (Test-Path "C:\Program Files\Tesseract-OCR\tesseract.exe") { # Escaped backslashes
        $tesseractInstallDir = "C:\Program Files\Tesseract-OCR" # Escaped backslashes
    }
}

$tessdataFound = $false
if ($tessdataPrefix -and (Test-Path (Join-Path $tessdataPrefix "tessdata\eng.traineddata"))) { # Escaped backslash
    Write-Host "Tesseract OCR data found via TESSDATA_PREFIX: $tessdataPrefix"
    $tessdataFound = $true
} elseif ($tesseractInstallDir -and (Test-Path (Join-Path $tesseractInstallDir "tessdata\eng.traineddata"))) { # Escaped backslash
    Write-Host "Tesseract OCR data found in installation directory: $(Join-Path $tesseractInstallDir "tessdata")" # Escaped backslash
    $tessdataFound = $true
    if (-not $tessdataPrefix) {
        Write-Host "Consider setting the TESSDATA_PREFIX environment variable to '$tesseractInstallDir' if Tesseract has issues finding data."
    }
}


if (-not $tessdataFound) {
    Write-Warning "Tesseract OCR data (e.g., eng.traineddata) not found or TESSDATA_PREFIX not correctly set."
    Write-Host "Please install Tesseract OCR language data. If using Chocolatey: choco install tesseract-ocr-chi-sim tesseract-ocr-eng (or other languages)"
    Write-Host "Or download from: https://github.com/tesseract-ocr/tessdata and place them in a 'tessdata' directory."
    Write-Host "Then, ensure the TESSDATA_PREFIX environment variable is set to the parent directory of 'tessdata'."
    Write-Host "Example: Set TESSDATA_PREFIX to 'C:\Program Files\Tesseract-OCR' (if 'tessdata' is inside it)." # Escaped backslashes
    Write-Host "You can set environment variables via 'System Properties' > 'Environment Variables'."
}

# Check for ffmpeg
Write-Host "Checking for ffmpeg..."
$ffmpegInstalled = $false
if (Test-CommandExists ffmpeg) {
    Write-Host "ffmpeg found in PATH."
    $ffmpegInstalled = $true
} else {
    Write-Warning "ffmpeg not found in PATH."
    Write-Host "Please install ffmpeg. Recommended method is using Chocolatey: choco install ffmpeg"
    Write-Host "Ensure ffmpeg's bin directory is added to your PATH."
    Write-Host "Then re-run this script."
}


Write-Host "`n--- Environment Variable Instructions ---"
Write-Host "Please ensure the following environment variables are set if they were not automatically configured or if you use custom paths:"
Write-Host ""
Write-Host "1. TESSDATA_PREFIX: Should point to the parent directory of your 'tessdata' folder."
Write-Host "   (e.g., if eng.traineddata is in 'C:\Program Files\Tesseract-OCR\tessdata\', set TESSDATA_PREFIX='C:\Program Files\Tesseract-OCR\')." # Escaped backslashes
Write-Host "   Current value of TESSDATA_PREFIX: '$tessdataPrefix'"
Write-Host ""
Write-Host "2. FFMPEG_PATH: If ffmpeg is not in your system's PATH, set this variable to the full path of the ffmpeg.exe executable."
Write-Host "   Example: \$env:FFMPEG_PATH = 'C:\ffmpeg\bin\ffmpeg.exe'" # Escaped backslashes
Write-Host ""
Write-Host "3. TMP_PATH (Optional): If you wish to use a specific temporary folder for ffmpeg operations, set this variable."
Write-Host "   Example: \$env:TMP_PATH = 'C:\Users\YourUser\AppData\Local\Temp\ffmpeg_tmp'" # Escaped backslashes
Write-Host ""
Write-Host "You can set these permanently via 'System Properties' > 'Advanced system settings' > 'Environment Variables'."
Write-Host "For the current session, you can use: \$env:VARIABLE_NAME = "value""
Write-Host ""

# Build .NET project
Write-Host "Building the .NET project (ClipHunta2)..."
if (Test-CommandExists dotnet) {
    dotnet build ClipHunta2.csproj
    if ($LASTEXITCODE -eq 0) {
        Write-Host ".NET project built successfully."
    } else {
        Write-Error "Failed to build the .NET project. Please check for errors above."
    }
} else {
    Write-Error ".NET SDK (dotnet command) not found. Cannot build project."
}

Write-Host "`nSetup script finished. Please review any messages above and ensure all dependencies are correctly installed and environment variables are set."
Write-Host "You can now try running the application as described in the Readme."
Write-Host "To run this script again, open PowerShell, navigate to this directory, and run: .\setup.ps1" # Escaped backslash
Write-Host "You might need to set the execution policy: Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass"
