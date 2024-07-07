# clipHunta

Welcome to the clipHunta, a razor-sharp application crafted to slice your downloaded Twitch VOD files based on
where the real action begins!

Channel your inner editor and cut through the unwanted segments, leaving only the best parts of your gaming highlights.
No more sifting through hours of footage looking for die-hard headshots or epic multikills!

## 📜 Table of Contents

1. [Getting Started](#getting-started)
2. [How To Use](#how-to-use)

## 🌐 Getting Started


* Install Tesseract https://github.com/tesseract-ocr/tessdoc 

* Download Tesseract Data https://github.com/tesseract-ocr/tessdata 

* Set Env Varaiable `TESSERACT_DATA`

* Use Choco to install ffmpeg. 
  * If you did not do this, set the ENV variable `FFMPEG_PATH`.
  * If you wish to use a different tmp folder for ffmpeg set `TMP_PATH` ENV Variable

Make sure you have .net8 for windows installed. 

```bash

dotnet build
```
 

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

