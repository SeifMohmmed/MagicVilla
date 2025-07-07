# 🏡 MagicVilla Project

[![License: MIT](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE.md)
[![.NET 9](https://img.shields.io/badge/.NET-9-blue.svg)](https://dotnet.microsoft.com/download)
[![Build](https://img.shields.io/badge/build-passing-brightgreen.svg)]()
[![Swagger UI](https://img.shields.io/badge/API-Docs-blue.svg)](https://localhost:<port>/swagger)

**MagicVilla** is a modern web application for managing villa listings, built with **ASP.NET Core (.NET 9)** and **Razor Pages**. It delivers a fully‑featured, versioned RESTful API, secure JWT‑based authentication, role‑based authorization, and a clean, responsive front‑end interface.

---

## 📑 Table of Contents

* [✨ Features](#-features)
* [🧱 Project Structure](#-project-structure)
* [🧰 Tech Stack](#-tech-stack)
* [🚀 Getting Started](#-getting-started)

  * [✅ Prerequisites](#-prerequisites)
  * [⚙️ Setup](#️-setup)
* [📡 API Endpoints](#-api-endpoints)

  * [🔄 Version 1 (v1)](#-version1v1)
  * [📸 Version 2 (v2)](#-version2v2)
* [🔐 Authentication & Authorization](#-authentication--authorization)
* [⚙️ Configuration](#️-configuration)
* [🛠️ Development Notes](#-development-notes)
* [🖼️ Screenshots](#-screenshots)
* [📄 License](#-license)
* [🤝 Contributing](#-contributing)
* [📬 Contact](#-contact)

---

## ✨ Features

- 🏠 **Villa Management:** Create, update, delete, and list villas with image support and amenity details.  
- 🌐 **RESTful API:** Versioned API (`v1`, `v2`) for backward compatibility.  
- 🔐 **Authentication:** Secure JWT‑based authentication with login, registration, and logout endpoints.  
- 👥 **Role‑Based Access:** Admin and Customer roles with tailored permissions.  
- 🖥️ **Razor Pages UI:** Clean front‑end with cookie‑based authentication.  
- 🔍 **Pagination & Filtering:** Filter villas by occupancy, search terms, and paginate results.  
- 🖼️ **Image Uploads:** Upload and manage villa images via API or web.  
- 🚨 **Error Handling:** Unified API response format using `APIResponse`.  
- 📘 **Swagger/OpenAPI:** Built‑in API documentation and testing via Swagger UI.  

---

## 🧱 Project Structure

```text
MagicVilla/
├── MagicVilla.API/          # ASP.NET Core Web API (v1 & v2)
│   ├── Controllers/
│   ├── Data/                # DbContext & migrations
│   ├── Models/              # Domain models / DTOs
│   ├── Services/            # Business logic layer
│   └── Program.cs           # Entry‑point
├── MagicVilla_Web/          # Razor Pages front‑end
│   ├── Pages/
│   ├── Services/            # Typed HTTP clients
│   └── wwwroot/             # Static assets (CSS/JS/images)
├── MagicVilla_Utility/      # Shared helpers & constants
└── README.md                # You‑re‑reading‑it
```

---

## 🧰 Tech Stack

* **.NET 9 / C# 13** — core runtime & language
* **ASP.NET Core Web API** — high‑performance REST services
* **Razor Pages** — page‑centric server‑side UI
* **Entity Framework Core** + **SQL Server** — data persistence
* **AutoMapper** — object‑object mapping
* **JWT Authentication** — stateless security tokens
* **Swagger / Swashbuckle** — interactive API docs
* **jQuery Validation** — client‑side form validation (MIT)

---

## 🚀 Getting Started

### ✅ Prerequisites

* [.NET 9 SDK](https://dotnet.microsoft.com/download) ➜ `dotnet --version` ≥ 9.0
* **SQL Server** (Developer / Express / Docker)
* **Visual Studio 2022** or **VS Code** with C# extensions

### ⚙️ Setup

1. **Clone the repository**

   ```bash
   git clone https://github.com/<your‑org>/MagicVilla.git
   cd MagicVilla
   ```

2. **Restore dependencies**

   ```bash
   dotnet restore
   ```

3. **Configure connection strings**

   Edit `MagicVilla.API/appsettings.json` (and `MagicVilla_Web/appsettings.json`) and set **ConnectionStrings\:DefaultConnection** to your SQL Server instance.

4. **Apply migrations & seed data**

   ```bash
   dotnet ef database update --project MagicVilla.API
   ```

5. **Run the API**

   ```bash
   dotnet run --project MagicVilla.API
   ```

6. **Run the Web App (optional)**

   ```bash
   dotnet run --project MagicVilla_Web
   ```

7. **Access the applications**

   * **Swagger UI** ➜ [https://localhost\:PORT/swagger](https://localhost:PORT/swagger)
   * **Web UI** ➜ [https://localhost\:PORT/](https://localhost:PORT/)

---

## 📡 API Endpoints

### 🔄 Version 1 (v1)

| Method | Endpoint                | Description                     | Role      |
| ------ | ----------------------- | ------------------------------- | --------- |
| GET    | `/api/v1/VillaAPI`      | List villas (filter & paginate) | Anyone    |
| GET    | `/api/v1/VillaAPI/{id}` | Get villa by ID                 | Anyone    |
| POST   | `/api/v1/VillaAPI`      | Create villa                    | **Admin** |
| PUT    | `/api/v1/VillaAPI/{id}` | Replace villa                   | **Admin** |
| PATCH  | `/api/v1/VillaAPI/{id}` | Partial update                  | **Admin** |
| DELETE | `/api/v1/VillaAPI/{id}` | Delete villa                    | **Admin** |

### 📸 Version 2 (v2)

All **v1** endpoints plus:

| Method | Endpoint                      | Description                                        |
| ------ | ----------------------------- | -------------------------------------------------- |
| POST   | `/api/v2/VillaAPI/{id}/image` | Upload / replace villa image (multipart/form‑data) |

---

## 🔐 Authentication & Authorization

| Flow           | Details                                                                                                            |
| -------------- | ------------------------------------------------------------------------------------------------------------------ |
| **JWT Tokens** | Issued at `/api/v1/Auth/login` & `/api/v1/Auth/register`. Include in `Authorization: Bearer <token>` header.       |
| **Roles**      | `Admin` (full CRUD), `Customer` (read‑only)                                                                        |
| **Web UI**     | Stores JWT in a secure, **HttpOnly cookie** for API calls; Razor Pages are protected via `[Authorize]` attributes. |

**Auth Endpoints**

| Method | Endpoint                | Purpose                   |
| ------ | ----------------------- | ------------------------- |
| POST   | `/api/v1/Auth/login`    | User login                |
| POST   | `/api/v1/Auth/register` | Register new user         |
| POST   | `/api/v1/Auth/logout`   | Revoke JWT & clear cookie |

---

## ⚙️ Configuration

| Key                                   | File               | Purpose                  |
| ------------------------------------- | ------------------ | ------------------------ |
| `ConnectionStrings:DefaultConnection` | `appsettings.json` | SQL Server connection    |
| `JWTSettings:Key`                     | `appsettings.json` | Symmetric signing key    |
| `JWTSettings:Issuer` / `Audience`     | `appsettings.json` | Token validation         |
| `ImageSettings:Path`                  | `appsettings.json` | Local image storage path |

---

## 🛠️ Development Notes

* Written in **.NET 9** using new **HTTP/3** and **output caching** middleware.
* Implements **Repository** and **Unit‑of‑Work** patterns for data access.
* Uses **Layered Architecture**: API ↔ Service ↔ Repository ↔ Data.
* Built‑in protection against **XSS**, **CSRF**, secure headers, and **rate limiting**.
* Lazy loading enabled for navigation properties.
* Consistent error schema via `APIResponse<T>`.

---

## 🖼️ Screenshots

Add your screenshots to `screenshots/` and link them here.

```md
![Home Page](screenshots/home.png)
![Admin View](screenshots/admin.png)
```

---

