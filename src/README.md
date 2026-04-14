# User Management Service

A C# ASP.NET Core 8.0 microservice for managing user identities, authentication, and authorization.

## Service Information

- **Organization ID**: `95bd4e80-e002-4fe5-ab71-fa85aad9fec8`
- **Project ID**: `69a9b4a9-d13f-47a0-90c5-817ba294c001`
- **Service ID**: `USR-MNG-01`
- **Service Name**: User Management Service

## Features

- User registration, retrieval, update, and deletion via RESTful APIs
- User authentication (login) with JWT token management
- Email verification with verification codes
- Role management and authorization enforcement
- Redis caching for sessions and tokens
- PostgreSQL for persistent user data
- Health check endpoint for service monitoring
- Standardized error handling and validation

## API Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/v1/users` | List all users (paginated) | Yes |
| GET | `/api/v1/users/{id}` | Get user by ID | Yes |
| POST | `/api/v1/users` | Create new user | No |
| PUT | `/api/v1/users/{id}` | Update user | Yes |
| DELETE | `/api/v1/users/{id}` | Delete user | Yes |
| POST | `/api/v1/auth/login` | Authenticate user | No |
| POST | `/api/v1/auth/verify/send` | Send verification code | No |
| POST | `/api/v1/auth/verify/validate` | Validate verification code | No |
| GET | `/health` | Health check | No |

## Data Models

### User
```json
{
  "id": "uuid",
  "name": "string",
  "email": "string",
  "role": "string",
  "emailVerified": "boolean",
  "createdAt": "datetime"
}
```

### ErrorResponse
```json
{
  "error": "string",
  "message": "string",
  "statusCode": "integer",
  "timestamp": "datetime",
  "traceId": "string",
  "validationErrors": {}
}
```

### Pagination
```json
{
  "totalItems": "integer",
  "totalPages": "integer",
  "currentPage": "integer",
  "itemsPerPage": "integer"
}
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Docker and Docker Compose (for containerized deployment)
- PostgreSQL 16+
- Redis 7+

### Running Locally

1. Clone the repository
2. Navigate to the `src` directory
3. Update `appsettings.Development.json` with your connection strings
4. Run the application:

```bash
cd src/UserManagementService
dotnet run
```

### Running with Docker Compose

```bash
cd src
docker-compose up -d
```

This will start:
- User Management Service on port 8080
- PostgreSQL on port 5432
- Redis on port 6379

### Running Tests

```bash
cd src
dotnet test
```

## Configuration

Configuration is managed through `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=usermanagement;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "Secret": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!",
    "Issuer": "UserManagementService",
    "Audience": "UserManagementService",
    "ExpirationMinutes": "60"
  }
}
```

## Project Structure

```
src/
├── UserManagementService/
│   ├── Controllers/           # REST API controllers
│   ├── Data/                  # Database context
│   ├── Middleware/            # Error handling middleware
│   ├── Models/                # Domain models and DTOs
│   ├── Repositories/          # Data access layer
│   ├── Services/              # Business logic layer
│   ├── Program.cs             # Application entry point
│   ├── appsettings.json       # Configuration
│   └── Dockerfile             # Container definition
├── UserManagementService.Tests/
│   ├── Controllers/           # Controller tests
│   └── Services/              # Service tests
├── docker-compose.yml         # Container orchestration
├── init-db.sql                # Database initialization
└── README.md                  # This file
```

## Security

- JWT Bearer authentication for protected endpoints
- Password hashing using SHA256
- Input validation on all endpoints
- Standardized error responses (no sensitive data leakage)

## TODOs

- [ ] Integrate with email service for sending verification codes
- [ ] Implement rate limiting and brute-force protection
- [ ] Publish UserCreatedEvent to Kafka on registration
- [ ] Integrate with File Storage Service for user files
- [ ] Add OIDC integration for federated authentication

## License

Proprietary - All rights reserved.
