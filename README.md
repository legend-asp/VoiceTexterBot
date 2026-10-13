# 🎙️ VoiceTexterBot — High-Throughput Speech-to-Text Telegram Bot

[![Framework](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)]()
[![Telegram](https://img.shields.io/badge/Telegram_Bot-API_22-2ca5e0?style=for-the-badge&logo=telegram&logoColor=white)]()
[![Engine](https://img.shields.io/badge/Offline_Engine-Vosk_Speech_Recognition-success?style=for-the-badge)]()
[![Media](https://img.shields.io/badge/Audio-FFmpegCore-007808?style=for-the-badge&logo=ffmpeg&logoColor=white)]()

> An offline, zero-cloud-cost Telegram bot that converts voice messages into text across multiple languages using Vosk acoustic models and FFmpeg.

---

## ⚡ Architectural Design

```mermaid
flowchart LR
    User([Telegram User]) -->|Sends Voice Note .ogg| Bot[Telegram Bot Client]
    Bot --> Worker[AudioFileHandler]
    Worker -->|Convert OGG to WAV 16kHz| FFmpeg[FFmpegCore Process]
    FFmpeg --> Model[Vosk Offline Recognizer]
    Model --> Text[Extracted Text]
    Text -->|Send Response| User
```

---

## 🛡️ Key Features

- **🔒 Offline & Private:** Speech-to-text processing occurs entirely locally on the server via Vosk models.
- **🌐 Multilingual Support:** Dynamic language detection and switching (Russian, English, French, Polish).
- **🧵 Concurrency & Isolation:** Thread-safe temporary file management using unique GUIDs to prevent race conditions.
- **🧼 Automatic Cleanup:** Guaranteed deletion of temporary audio files via lifecycle hooks.

---

## 🚀 Quickstart

```bash
git clone https://github.com/yandereye1/VoiceTexterBot.git
cd VoiceTexterBot/VoiceTexterBot1
dotnet run
```

> **Note:** Set your Telegram bot token in `appsettings.json` or pass it as an environment variable before starting.

---

## 👨‍💻 Author
GitHub: [@yandereye1](https://github.com/yandereye1)
