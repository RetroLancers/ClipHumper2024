#!/bin/bash

echo "Starting ClipHunta Setup for Linux/macOS..."

# Function to check if a command exists
command_exists () {
    type "$1" &> /dev/null ;
}

# Check for .NET 8 SDK
echo "Checking for .NET 8 SDK..."
if ! command_exists dotnet || ! dotnet --list-sdks | grep -q "8\.0"; then
    echo ".NET 8 SDK not found."
    echo "Please install the .NET 8 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0"
    echo "Then re-run this script."
    # exit 1 # Optionally exit if critical
else
    echo ".NET 8 SDK found."
fi

# Check for Python 3
echo "Checking for Python 3..."
if ! command_exists python3; then
    echo "Python 3 not found."
    echo "Please install Python 3. For example, on Debian/Ubuntu: sudo apt update && sudo apt install python3 python3-pip"
    echo "For macOS with Homebrew: brew install python"
    echo "Then re-run this script."
    # exit 1
else
    echo "Python 3 found."
fi

# Check for pip (Python package installer)
echo "Checking for pip..."
if ! command_exists pip3; then
    echo "pip3 not found. It's usually installed with Python 3."
    echo "Please ensure pip3 is installed. For example, on Debian/Ubuntu: sudo apt install python3-pip"
    # exit 1
else
    echo "pip3 found."
fi

# Install twitch-dl
echo "Attempting to install/update twitch-dl..."
if command_exists pip3; then
    pip3 install --user --upgrade twitch-dl
    echo "Make sure ~/.local/bin is in your PATH if you installed with --user and twitch-dl command is not found."
    echo "You might need to run: export PATH=\$PATH:~/.local/bin"
else
    echo "pip3 not found, cannot install twitch-dl automatically."
fi


# Check for Tesseract OCR
echo "Checking for Tesseract OCR..."
if ! command_exists tesseract; then
    echo "Tesseract OCR not found."
    echo "Please install Tesseract OCR. For example:"
    echo "  Debian/Ubuntu: sudo apt update && sudo apt install tesseract-ocr"
    echo "  macOS (using Homebrew): brew install tesseract"
    echo "  Fedora: sudo dnf install tesseract"
    echo "Then re-run this script."
    # exit 1
else
    echo "Tesseract OCR found."
fi

# Check for Tesseract OCR Data
echo "Checking for Tesseract OCR language data (e.g., eng)..."
# This is a basic check, actual data path can vary.
# On Linux, it's often /usr/share/tesseract-ocr/4.00/tessdata/ or /usr/local/share/tessdata/
# On macOS (Homebrew), it's often /usr/local/Cellar/tesseract/<version>/share/tessdata/ or /opt/homebrew/share/tessdata/
TESSDATA_PATH_GUESS_LINUX="/usr/share/tesseract-ocr/4.00/tessdata"
TESSDATA_PATH_GUESS_MACOS_BREW_INTEL="/usr/local/share/tessdata"
TESSDATA_PATH_GUESS_MACOS_BREW_ARM="/opt/homebrew/share/tessdata"

if [[ -d "$TESSDATA_PATH_GUESS_LINUX" && -f "$TESSDATA_PATH_GUESS_LINUX/eng.traineddata" ]] || \
   [[ -d "$TESSDATA_PATH_GUESS_MACOS_BREW_INTEL" && -f "$TESSDATA_PATH_GUESS_MACOS_BREW_INTEL/eng.traineddata" ]] || \
   [[ -d "$TESSDATA_PATH_GUESS_MACOS_BREW_ARM" && -f "$TESSDATA_PATH_GUESS_MACOS_BREW_ARM/eng.traineddata" ]] || \
   [ -n "$TESSERACT_DATA_PREFIX" ] || [ -n "$TESSDATA_PREFIX" ]; then # TESSDATA_PREFIX is the actual env var tesseract uses
    echo "Tesseract OCR data seems to be present or TESSDATA_PREFIX is set."
else
    echo "Tesseract OCR data (e.g., eng.traineddata) not found in common locations."
    echo "Please install the Tesseract OCR language data. For example:"
    echo "  Debian/Ubuntu: sudo apt install tesseract-ocr-eng"
    echo "  macOS (using Homebrew): brew install tesseract-lang (this installs all langs, or pick specific ones)"
    echo "  Or download from: https://github.com/tesseract-ocr/tessdata and place them in a 'tessdata' directory."
    echo "Then, ensure the TESSDATA_PREFIX environment variable is set to the parent directory of 'tessdata'."
    echo "Example: export TESSDATA_PREFIX=/path/to/your/tesseract/data_directory (where /path/to/your/tesseract/data_directory/tessdata exists)"
    echo "Re-run this script after installation and setting the variable."
fi

# Check for ffmpeg
echo "Checking for ffmpeg..."
if ! command_exists ffmpeg; then
    echo "ffmpeg not found."
    echo "Please install ffmpeg. For example:"
    echo "  Debian/Ubuntu: sudo apt update && sudo apt install ffmpeg"
    echo "  macOS (using Homebrew): brew install ffmpeg"
    echo "  Fedora: sudo dnf install ffmpeg"
    echo "Then re-run this script."
    # exit 1
else
    echo "ffmpeg found."
fi

echo ""
echo "--- Environment Variable Instructions ---"
echo "Please ensure the following environment variables are set if they were not automatically configured or if you use custom paths:"
echo ""
echo "1. TESSDATA_PREFIX: Should point to the parent directory of your 'tessdata' folder (e.g., if eng.traineddata is in /usr/share/tesseract-ocr/4.00/tessdata/, set TESSDATA_PREFIX=/usr/share/tesseract-ocr/4.00/)."
echo "   The script has checked for common locations, but verify if Tesseract reports issues."
echo "   Current value of TESSDATA_PREFIX: '$TESSDATA_PREFIX'"
echo ""
echo "2. FFMPEG_PATH: If ffmpeg is not in your system's PATH, set this variable to the full path of the ffmpeg executable."
echo "   The script checks if ffmpeg is in PATH. If it was found, you likely don't need to set this."
echo "   Example: export FFMPEG_PATH=/opt/ffmpeg/ffmpeg"
echo ""
echo "3. TMP_PATH (Optional): If you wish to use a specific temporary folder for ffmpeg operations, set this variable."
echo "   Example: export TMP_PATH=/path/to/your/tmp_folder"
echo ""
echo "You can set these in your shell configuration file (e.g., ~/.bashrc, ~/.zshrc) to make them permanent."
echo "Example for ~/.bashrc:"
echo "  export TESSDATA_PREFIX=/usr/local/share/"
echo "  export FFMPEG_PATH=/usr/local/bin/ffmpeg"
echo ""

# Build .NET project
echo "Building the .NET project (ClipHunta2)..."
if command_exists dotnet; then
    dotnet build ClipHunta2.csproj
    if [ $? -eq 0 ]; then
        echo ".NET project built successfully."
    else
        echo "Failed to build the .NET project. Please check for errors above."
        # exit 1
    fi
else
    echo ".NET SDK (dotnet command) not found. Cannot build project."
    # exit 1
fi

echo ""
echo "Setup script finished. Please review any messages above and ensure all dependencies are correctly installed and environment variables are set."
echo "You can now try running the application as described in the Readme."
