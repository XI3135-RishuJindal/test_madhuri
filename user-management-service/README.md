# User Management Service

A secure, reliable, and scalable microservice for managing user identities, authentication, and authorization.

## Overview

The User Management Service provides:
- User registration, retrieval, update, and deletion via RESTful APIs
- User authentication (login) with JWT token management
- Email verification with Redis-backed verification codes
- Role-based authorization
- Session/token caching with Redis
- Event publishing to Kafka (optional)

## Technology Stack

- **Framework**: Spring Boot 3.2
- **Language**: Java 17
- **Database**: PostgreSQL
- **Cache**: Redis
- **Authentication**: OAuth2/JWT
- **Event Streaming**: Apache Kafka (optional)

## API Endpoints

### User Management

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/v1/users` | List all users (paginated) | Yes |
| GET | `/api/v1/users/{id}` | Get user by ID | Yes |
| POST | `/api/v1/users` | Create new user | No |
| PUT | `/api/v1/users/{id}` | Update user | Yes |
| DELETE | `/api/v1/users/{id}` | Delete user | Yes |
| POST | `/api/v1/users/verify` | Verify user email | No |

### Authentication

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/v1/auth/login` | User login | No |
| POST | `/api/v1/auth/logout` | User logout | Yes |

### Health

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
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
  "active": "boolean",
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

### ErrorResponse
```json
{
  "error": "string",
  "message": "string",
  "statusCode": "integer",
  "path": "string",
  "timestamp": "datetime",
  "fieldErrors": [
    {
      "field": "string",
      "message": "string"
    }
  ]
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

- Java 17+
- Maven 3.9+
- Docker & Docker Compose (for containerized deployment)
- PostgreSQL 15+
- Redis 7+

### Local Development

1. **Start dependencies with Docker Compose:**
   ```bash
   docker-compose up -d postgres redis
   ```

2. **Run the application:**
   ```bash
   mvn spring-boot:run
   ```

3. **Or build and run the JAR:**
   ```bash
   mvn clean package
   java -jar target/user-management-service-1.0.0-SNAPSHOT.jar
   ```

### Docker Deployment

1. **Build and run all services:**
   ```bash
   docker-compose up --build
   ```

2. **With Kafka enabled:**
   ```bash
   docker-compose --profile kafka up --build
   ```

### Configuration

Environment variables:

| Variable | Description | Default |
|----------|-------------|---------|
| `DB_HOST` | PostgreSQL host | localhost |
| `DB_PORT` | PostgreSQL port | 5432 |
| `DB_NAME` | Database name | userdb |
| `DB_USERNAME` | Database username | postgres |
| `DB_PASSWORD` | Database password | postgres |
| `REDIS_HOST` | Redis host | localhost |
| `REDIS_PORT` | Redis port | 6379 |
| `JWT_SECRET` | JWT signing secret (Base64) | - |
| `JWT_EXPIRATION` | JWT expiration (ms) | 86400000 |
| `KAFKA_BOOTSTRAP_SERVERS` | Kafka servers | localhost:9092 |

## Testing

Run unit tests:
```bash
mvn test
```

Run integration tests:
```bash
mvn verify
```

## API Examples

### Create User
```bash
curl -X POST http://localhost:8080/api/v1/users \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe",
    "email": "john@example.com",
    "password": "securePassword123"
  }'
```

### Login
```bash
curl -X POST http://localhost:8080/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "securePassword123"
  }'
```

### Get User (Authenticated)
```bash
curl -X GET http://localhost:8080/api/v1/users/{id} \
  -H "Authorization: Bearer <jwt-token>"
```

### Health Check
```bash
curl http://localhost:8080/health
```

## Security

- All endpoints except registration, login, verification, and health check require JWT authentication
- Passwords are hashed using BCrypt
- JWT tokens are cached in Redis for session management
- Token blacklisting supported for logout functionality
- Input validation on all endpoints

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    User Management Service                   │
├─────────────────────────────────────────────────────────────┤
│  Controllers                                                 │
│  ├── UserController (CRUD operations)                       │
│  ├── AuthController (Login/Logout)                          │
│  └── HealthController (Health check)                        │
├─────────────────────────────────────────────────────────────┤
│  Services                                                    │
│  ├── UserManagementService (Business logic)                 │
│  ├── AuthenticationService (JWT auth)                       │
│  ├── UserVerificationService (Email verification)           │
│  ├── TokenCacheService (Redis caching)                      │
│  └── EventPublisherService (Kafka events)                   │
├─────────────────────────────────────────────────────────────┤
│  Security                                                    │
│  ├── JwtTokenProvider (Token generation/validation)         │
│  ├── JwtAuthenticationFilter (Request filter)               │
│  └── SecurityConfig (Security configuration)                │
├─────────────────────────────────────────────────────────────┤
│  Repository                                                  │
│  └── UserRepository (PostgreSQL)                            │
└─────────────────────────────────────────────────────────────┘
         │                    │                    │
         ▼                    ▼                    ▼
    ┌─────────┐         ┌─────────┐         ┌─────────┐
    │PostgreSQL│         │  Redis  │         │  Kafka  │
    └─────────┘         └─────────┘         └─────────┘
```

## License

Proprietary - All rights reserved.
