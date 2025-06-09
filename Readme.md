# clipHunta

Welcome to the clipHunta, a razor-sharp application crafted to slice your downloaded Twitch VOD files based on
where the real action begins!

Channel your inner editor and cut through the unwanted segments, leaving only the best parts of your gaming highlights.
No more sifting through hours of footage looking for die-hard headshots or epic multikills!

## 📜 Table of Contents

1. [Getting Started](#getting-started)
2. [How To Use](#how-to-use)

## 🌐 Getting Started

This project includes setup scripts to help you configure your environment for Windows, Linux, and macOS. It also supports containerized setup using Docker.

### Prerequisites

The setup scripts will attempt to check for the following dependencies and guide you if they are missing:

*   **.NET 8 SDK**: Required to build and run the C# application.
*   **Python 3**: Required for the `twitch-dl` utility.
*   **pip**: Python package installer, used to install `twitch-dl`.
*   **Tesseract OCR**: Used for optical character recognition.
    *   **Tesseract Language Data**: (e.g., `eng.traineddata` for English).
*   **ffmpeg**: Required for video processing.
*   **twitch-dl**: Python utility to download Twitch VODs. (The scripts will attempt to install this via pip).

### Using Setup Scripts

#### Linux/macOS (`setup.sh`)

1.  Open your terminal.
2.  Navigate to the root directory of the project.
3.  Make the script executable: `chmod +x setup.sh`
4.  Run the script: `./setup.sh`
5.  The script will check for dependencies, suggest installation commands if needed, and then build the .NET project. Follow any on-screen instructions.

#### Windows (`setup.ps1`)

1.  Open PowerShell (it's recommended to run as Administrator if you need to install software via Chocolatey, though the script itself doesn't force admin rights).
2.  Navigate to the root directory of the project.
3.  You may need to adjust your script execution policy. To allow the script to run for the current session, you can use:
    `Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass`
4.  Run the script: `.\setup.ps1`
5.  The script will check for dependencies, suggest installation commands (often recommending Chocolatey), and then build the .NET project. Follow any on-screen instructions.

### Environment Variables

Properly setting environment variables is crucial for ClipHunta to function correctly. The setup scripts will provide guidance, but you may need to set or verify them manually, especially if you have custom installation paths.

*   **`TESSDATA_PREFIX`**: This variable must point to the parent directory of your `tessdata` folder (which contains `.traineddata` files like `eng.traineddata`).
    *   **Linux/macOS Example**: If your data is in `/usr/share/tesseract-ocr/4.00/tessdata/`, set `export TESSDATA_PREFIX=/usr/share/tesseract-ocr/4.00/`.
    *   **Windows Example**: If your data is in `C:\Program Files\Tesseract-OCR	essdata\`, set `TESSDATA_PREFIX` to `C:\Program Files\Tesseract-OCR` in your Environment Variables settings.
*   **`FFMPEG_PATH`** (Optional): Only needed if `ffmpeg` is not in your system's PATH. Set this to the full path of the `ffmpeg` executable.
    *   **Linux/macOS Example**: `export FFMPEG_PATH=/opt/ffmpeg/ffmpeg`
    *   **Windows Example**: `$env:FFMPEG_PATH = "C:fmpeginfmpeg.exe"` (in PowerShell for the current session, or set system-wide).
*   **`TMP_PATH`** (Optional): Set this if you want `ffmpeg` to use a specific temporary directory for its operations.
    *   **Linux/macOS Example**: `export TMP_PATH=/mnt/my_temp_space`
    *   **Windows Example**: `$env:TMP_PATH = "C:\my_temp_space"`

You can set these variables in your shell's profile file (e.g., `~/.bashrc`, `~/.zshrc` for Linux/macOS) or system environment variables settings (for Windows) to make them permanent.

### Using Docker

For a containerized setup:

1.  **Ensure Docker is installed** on your system.
2.  **Build the Docker image**:
    ```bash
    docker build -t cliphunta .
    ```
3.  **Run the Docker container**:
    When running the container, you might need to pass environment variables if the defaults within the image are not suitable for your Tesseract data location (though the image attempts to install data to standard paths). You may also want to map volumes for input/output files and a temporary path.
    ```bash
    # Example:
    # docker run -it --rm \
    #   -v /path/to/your/videos:/app/videos \
    #   -v /path/to/your/clips:/app/clips \
    #   -v /path/to/your/temp:/app/temp \
    #   -e TMP_PATH="/app/temp" \
    #   cliphunta dotnet ClipHunta2.dll <your_video_file_in_container>
    ```
    Replace `/path/to/your/...` with actual paths on your host machine. The entrypoint is `dotnet ClipHunta2.dll`, so you'll append your application arguments after `cliphunta`.
    The `TESSDATA_PREFIX` is generally handled by the `tesseract-ocr-eng` package within the Docker image. `ffmpeg` is installed in the PATH.

## 🕹️ How To Use

Using clipHunta is easy and straightforward.

```bash
 twitch-dl download [OPTIONS] [IDS]
```

```bash

dotnet run <filename>

```

## Disclaimer 

Garbage in (non-vod files, non mkv or something opencv can't read) garbage out.

Works best on 1080p60 streams. You may have to adjust box position for other streams as I did not adjust for low-qual since it's 2024.

If someoene is streaming in less than 1080p or 720p don't even bother.
Bitrate and quality matter.

