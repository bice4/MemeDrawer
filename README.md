# 🧠 MemeDrawer

**MemeDrawer** is an experimental .NET Aspire–based project for generating and managing memes using a modern full-stack architecture — combining **React**, **ASP.NET Core API**, **SQLite**, and **Azure Blob Storage**.

> 🧩 This project was built **just for fun and exploration** — to experiment with .NET Aspire, distributed setup, and image generation pipelines.

---

## 🚀 Overview

MemeDrawer allows users to:
- Upload and store images in **Azure Blob Storage**
- Save meme metadata in a local **SQLite** database
- Generate memes dynamically via API requests with customizable options (text, style, etc.)
- View and edit meme templates through the **React UI**

Built using the new **.NET Aspire** application model — providing a modular, cloud-ready, and local-friendly architecture.

---

## 🧩 Tech Stack

| Layer | Technology |
|-------|-------------|
| Frontend | React + PrimeReact |
| Backend | ASP.NET Core Web API |
| Infrastructure | .NET Aspire |
| Database | SQLite |
| Storage | Azure Blob Storage |
| Image Processing | SkiaSharp (fast, native text rendering) |

---

## ⚙️ Features

- 🖼️ Generate memes on the fly  
- ☁️ Store images in Azure Blob Storage  
- 💾 Track metadata in SQLite  
- ⚡ High-performance text rendering with SkiaSharp  
- 🧩 Simple API for integration or automation  
- 🧠 Built with .NET Aspire orchestration  

---

## 🧰 API Example

**POST** `/api/ImageDrawer`

```json
{
  "imageId": "123",
  "topText": "HELLO WORLD",
  "bottomText": "FROM MEMEDRAWER"
}
```

## 🧑‍💻 Local Development

You can run MemeDrawer locally using Docker and .NET Aspire.

**🧱 Requirements**

- .NET 9 SDK
- Node.js (LTS)
- Docker Desktop
- Azure Storage Emulator or Azurite (Docker)
