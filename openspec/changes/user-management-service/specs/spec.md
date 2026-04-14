# User Management Service – Technical Specification

## Purpose
The User Management Service SHALL provide secure, reliable, and scalable management of user identities, authentication, and authorization for the platform.

---

### Requirement 1: User Account Management
#### Scenario: User Registration
- **Given** a valid user registration request,
- **When** the client calls `POST /api/v1/users` with required fields,
- **Then** the service SHALL create a new user, persist it, and return a 201 response.

#### Scenario: User Retrieval
- **Given** a valid user ID,
- **When** the client calls `GET /api/v1/users/{id}`,
- **Then** the service SHALL return user details if the user exists, or a 404 error if not.

#### Scenario: User Update
- **Given** an authenticated user,
- **When** the client calls `PUT /api/v1/users/{id}` with valid updates,
- **Then** the service SHALL update the user and return a 200 response.

#### Scenario: User Deletion
- **Given** an authenticated user or admin,
- **When** the client calls `DELETE /api/v1/users/{id}`,
- **Then** the service SHALL delete the user and return a 204 response.

---

### Requirement 2: Authentication and Session Management
#### Scenario: User Login
- **Given** valid credentials,
- **When** the client calls `POST /api/v1/auth/login`,
- **Then** the service SHALL authenticate the user and return a JWT token.

#### Scenario: Token Validation
- **Given** a protected endpoint,
- **When** the client provides a valid Bearer token,
- **Then** the service SHALL authorize access; otherwise, it SHALL return a 401 error.

---

### Requirement 3: Health Monitoring
#### Scenario: Health Check
- **Given** the service is running,
- **When** the client calls `GET /health`,
- **Then** the service SHALL return a 200 response indicating health.

---

### Requirement 4: Error Handling
- All endpoints SHALL return standard HTTP error codes.
- Error responses SHALL follow the documented structure:
  - `ErrorResponse`: `{ error: string, message: string, statusCode: integer }`
- Validation errors SHALL be clearly indicated.

---

### Technologies and Runtime Stack
- **Languages/Frameworks**: Spring Boot (Java)
- **Database**: PostgreSQL
- **Authentication**: OAuth2, JWT
- **Cache**: Redis
- **Protocols**: REST (JSON over HTTP)
- **Other**: Kafka (for analytics/events), File Storage Service

---

### Components
- **Controllers/Handlers**: Expose REST endpoints.
- **Domain Services**: UserManagementService, UserVerificationService.
- **Repositories**: UserRepository (PostgreSQL).
- **DTOs/Models**: User, UserDTO, VerificationRequest, VerificationResponse, ErrorResponse, Pagination.
- **Integration Clients**: Redis, File Storage, Kafka (for events).

---

### API Endpoints
- `GET /api/v1/users` – List all users.
- `GET /api/v1/users/{id}` – Get user details.
- `POST /api/v1/users` – Create user.
- `PUT /api/v1/users/{id}` – Update user.
- `DELETE /api/v1/users/{id}` – Delete user.
- `POST /api/v1/auth/login` – Authenticate user.
- `GET /health` – Health check.

---

### Data Models
- **User**: `{ id: string, name: string, email: string, createdAt: date-time }`
- **ErrorResponse**: `{ error: string, message: string, statusCode: integer }`
- **Pagination**: `{ totalItems: integer, totalPages: integer, currentPage: integer, itemsPerPage: integer }`
- **UserDTO, VerificationRequest, VerificationResponse, UserCreatedEvent**: As described in LLD.

---

### Interactions with Dependencies
- **PostgreSQL**: CRUD operations for users.
- **Redis**: Session/token caching.
- **File Storage Service**: For user files (if applicable).
- **Kafka**: Publish `UserCreatedEvent` (if required).
- **Identity Provider**: OIDC/OAuth2 flows for federated login.

---

### Key Flows
#### User Registration Flow (Example)
1. Client submits registration via `POST /api/v1/users`.
2. Controller validates input and calls UserManagementService.
3. UserManagementService creates User entity and persists via UserRepository.
4. UserVerificationService sends verification code (if required).
5. On success, returns 201 response.

#### User Login Flow
1. Client submits credentials via `POST /api/v1/auth/login`.
2. Service validates credentials against stored user.
3. On success, generates JWT token and returns to client.
4. Token is cached in Redis for session management.

---

### TODOs
- Clarify if UserCreatedEvent is published to Kafka on every registration.
- Confirm if File Storage Service is used for user profile images or documents.
- Specify rate limiting and brute-force protection requirements.

---