# GameStore API

A RESTful web API for managing a digital game storefront, built with ASP.NET Core 10. The project uses a 3-layer architecture (Controllers, Services, Repositories) and Entity Framework Core with PostgreSQL.

## Features
- CRUD operations for Games
- Get operations for Genres
- Input validation using Data Annotations
- N-Tier Architecture (Controller - Service - Repository pattern)
- Entity Framework Core migrations and data seeding
- PostgreSQL integration (Neon Tech)

## Tech Stack
- .NET 10.0
- ASP.NET Core Web API
- Entity Framework Core 10.0
- Npgsql (PostgreSQL provider)

## Project Structure
- `Controllers/`: HTTP request handling and routing.
- `Services/`: Business logic and DTO mapping.
- `Repositories/`: Data access layer and database operations.
- `Dtos/`: Data Transfer Objects for request/response contracts.
- `Models/`: Entity models mapped to the database.

## Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A PostgreSQL database (e.g., [Neon Tech](https://neon.tech/), Docker, or local install)

## Getting Started

1. **Clone the repository**
   ```bash
   git clone <your-repo-url>
   cd GameStoreAPI-WebAPI-FullCourse/GameStore.Api
   ```

2. **Configure Database Connection**
   Open `appsettings.json` and update the `GameStore` connection string with your PostgreSQL credentials:
   ```json
   "ConnectionStrings": {
     "GameStore": "Server=your_host;Database=your_db;User Id=your_user;Password=your_password;Ssl Mode=Require;"
   }
   ```

3. **Run the Application**
   The application will automatically apply any pending Entity Framework migrations and seed initial Genre data on startup.
   ```bash
   dotnet run
   ```
   The API will be available at `http://localhost:5217`.

## API Endpoints

### Games (`/games`)
| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/games` | Get all games |
| `GET` | `/games/{id}` | Get a specific game by ID |
| `POST` | `/games` | Create a new game |
| `PUT` | `/games/{id}` | Update an existing game |
| `DELETE` | `/games/{id}` | Delete a game |

### Genres (`/genres`)
| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/genres` | Get all available genres |

### Example Request Body (POST/PUT `/games`)
```json
{
  "name": "Super Mario Bros. Wonder",
  "genreId": 3,
  "price": 59.99,
  "releaseDate": "2023-10-20"
}
```

## Testing
You can test the endpoints using the provided `games.http` file via the VS Code REST Client extension, or by using tools like Postman or curl.
