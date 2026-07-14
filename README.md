# GameStore API - Production Ready

A RESTful web API for managing a digital game storefront, built with ASP.NET Core 10. The project uses a **3-layer architecture** (Controllers, Services, Repositories) and Entity Framework Core with PostgreSQL.

This API has been upgraded to a **Production-Ready** standard, featuring Real JWT Authentication, Advanced Pagination, Rate Limiting, and Global Exception Handling.

## ✨ Key Features
- **Authentication:** Secure user registration & login using JWT (JSON Web Tokens) and **BCrypt** password hashing.
- **Advanced Querying:** Filter, search, and paginate through Games effortlessly.
- **Partial Updates:** Efficiently update resources using `PATCH` requests.
- **Robust Architecture:** N-Tier Architecture (Controller - Service - Repository pattern) separating business logic from data access.
- **Production Safeguards:**
  - Standardized JSON responses for all errors via **Global Exception Handler** (RFC 7807).
  - Protection against SQL Injection using Parameterized Queries.
  - **Rate Limiting** to prevent Spam/DDoS.
  - Cross-Origin Resource Sharing (**CORS**) enabled for frontend integration.
- **Database Consistency:** PostgreSQL (Neon Tech) integration with Cascade Delete enabled.

## 🛠 Tech Stack
- **.NET 10.0** (ASP.NET Core Web API)
- **Entity Framework Core 10.0**
- **Npgsql** (PostgreSQL provider)
- **BCrypt.Net-Next** (Cryptography)
- **Microsoft.AspNetCore.OpenApi** (Swagger / Documentation)

## 🚀 Getting Started

1. **Clone the repository**
   ```bash
   git clone <your-repo-url>
   cd GameStoreAPI-WebAPI-FullCourse/GameStore.Api
   ```

2. **Configure Database Connection & JWT**
   Open `appsettings.json` and ensure your PostgreSQL credentials and JWT configurations are set:
   ```json
   "ConnectionStrings": {
     "GameStore": "Server=your_host;Database=your_db;User Id=your_user;Password=your_password;Ssl Mode=Require;"
   },
   "Jwt": {
     "Key": "super_secret_key_that_is_long_enough_for_hmac_sha256_production_ready_key_here",
     "Issuer": "https://api.gamestore.local",
     "Audience": "https://web.gamestore.local"
   }
   ```

3. **Apply Database Migrations**
   Unlike dev environments, migrations are applied manually for safety:
   ```bash
   dotnet ef database update
   ```

4. **Run the Application**
   ```bash
   dotnet run
   ```
   The API will be available at `http://localhost:5217`. You can also access the **OpenAPI Documentation** via the `/openapi/v1.json` endpoint (if using Scalar/Swagger UI).

## 📡 API Endpoints

### 🔐 Authentication (`/auth`)
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/auth/register` | Register a new user |
| `POST` | `/auth/login` | Login and receive a JWT Bearer token |

### 🎮 Games (`/games`)
| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| `GET` | `/games` | Get all games (Supports `page`, `pageSize`, `search`, `minPrice`, `maxPrice`) | ❌ No |
| `GET` | `/games/{id}` | Get a specific game by ID | ❌ No |
| `POST` | `/games` | Create a new game | 🔒 Yes |
| `PATCH` | `/games/{id}` | Update an existing game partially | 🔒 Yes |
| `DELETE` | `/games/{id}` | Delete a game | 🔒 Yes |

### 🏷 Genres (`/genres`)
| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| `GET` | `/genres` | Get all available genres | ❌ No |
| `POST` | `/genres` | Create a new genre | 🔒 Yes |
| `PATCH` | `/genres/{id}` | Update an existing genre partially | 🔒 Yes |
| `DELETE` | `/genres/{id}` | Delete a genre | 🔒 Yes |

## 🧪 Testing
You can test the endpoints using **Postman** by importing the provided `GameStore_Postman_Collection.json`. The collection includes pre-configured tests that automatically capture and apply the JWT Token across secure endpoints!
