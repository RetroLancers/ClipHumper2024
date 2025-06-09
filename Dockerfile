# Use .NET 8 SDK as the build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install dependencies: Python, pip, Tesseract, ffmpeg
USER root
RUN apt-get update && apt-get install -y --no-install-recommends \
    python3 \
    python3-pip \
    tesseract-ocr \
    tesseract-ocr-eng \
    ffmpeg \
 && rm -rf /var/lib/apt/lists/*

# Install twitch-dl using pip
RUN pip3 install --no-cache-dir twitch-dl

# Copy project files and restore dependencies
COPY ["ClipHunta2.csproj", "./"]
RUN dotnet restore "ClipHunta2.csproj"

COPY . .
WORKDIR "/src/"
RUN dotnet build "ClipHunta2.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "ClipHunta2.csproj" -c Release -o /app/publish

# Create the final runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

# Install runtime dependencies (ffmpeg, tesseract runtime libs if not covered by base, python for twitch-dl)
# Note: tesseract-ocr and ffmpeg also needed in runtime image. Python for twitch-dl.
USER root
RUN apt-get update && apt-get install -y --no-install-recommends \
    python3 \
    tesseract-ocr \
    ffmpeg \
 && rm -rf /var/lib/apt/lists/*

# Ensure TESSDATA_PREFIX is set correctly if tesseract-ocr-eng doesn't set it globally
# For standard installs, tesseract usually finds its data. If not, this ENV var is critical.
# The tesseract-ocr-eng package should place data in a location tesseract can find.
# ENV TESSDATA_PREFIX=/usr/share/tesseract-ocr/4.00/tessdata # Adjust if needed based on actual install path in the container

COPY --from=publish /app/publish .

# Expose necessary environment variables documentation (users will set these when running the container)
# ENV TESSERACT_DATA (already covered by TESSDATA_PREFIX or default paths)
# ENV FFMPEG_PATH (ffmpeg should be in PATH)
# ENV TMP_PATH (user can set this to map a volume for temp files)

# Entrypoint
ENTRYPOINT ["dotnet", "ClipHunta2.dll"]
