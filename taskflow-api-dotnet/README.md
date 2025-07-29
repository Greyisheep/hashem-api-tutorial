# TaskFlow API - .NET Implementation

A production-ready .NET 8 API implementing Domain-Driven Design, structured logging, security best practices, and modern API patterns.

## 🎯 Features

- **Domain-Driven Design (DDD)** - Clean architecture with rich domain models
- **Structured Logging** - Serilog with correlation IDs and structured data
- **Security First** - JWT authentication, authorization, and security headers
- **Envelope Response Pattern** - Consistent API responses with metadata
- **Docker Integration** - Complete containerized development environment
- **Database Integration** - Entity Framework Core with PostgreSQL
- **API Documentation** - OpenAPI/Swagger with comprehensive examples
- **Health Checks** - Production-ready monitoring endpoints
- **Input Validation** - FluentValidation with custom validators
- **Error Handling** - Global exception handling with structured errors

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- Docker Desktop
- Visual Studio 2022 or VS Code

### Running with Docker
```bash
# Clone and navigate to project
cd taskflow-api-dotnet

# Start all services
docker-compose up --build

# API will be available at: http://localhost:5000
# Swagger UI at: http://localhost:5000/swagger
# Health checks at: http://localhost:5000/health
```

### Running Locally
```bash
# Install dependencies
dotnet restore

# Run database migrations
dotnet ef database update

# Start the API
dotnet run --project src/TaskFlow.API

# API will be available at: http://localhost:5000
```

## 🏗️ Project Structure

```
TaskFlow.API/
├── src/
│   ├── TaskFlow.API/              # Web API layer
│   ├── TaskFlow.Application/      # Application services & DTOs
│   ├── TaskFlow.Domain/           # Domain entities & business logic
│   └── TaskFlow.Infrastructure/   # Data access & external services
├── tests/
│   ├── TaskFlow.API.Tests/        # Integration tests
│   ├── TaskFlow.Application.Tests/ # Unit tests
│   └── TaskFlow.Domain.Tests/     # Domain tests
├── docker-compose.yml             # Development environment
├── Dockerfile                     # API container
└── postman/                       # API collections
```

## 🔐 Security Features

- **JWT Authentication** - Secure token-based authentication
- **Role-Based Authorization** - Fine-grained permission control
- **Input Validation** - Comprehensive request validation
- **Security Headers** - Protection against common attacks
- **Rate Limiting** - Prevent API abuse
- **CORS Configuration** - Secure cross-origin requests
- **Password Hashing** - BCrypt with salt rounds

## 📊 API Endpoints

### Authentication
- `POST /api/auth/login` - User authentication
- `POST /api/auth/refresh` - Token refresh
- `POST /api/auth/logout` - User logout

### Tasks
- `GET /api/tasks` - List tasks
- `GET /api/tasks/{id}` - Get task details
- `POST /api/tasks` - Create new task
- `PUT /api/tasks/{id}` - Update task
- `DELETE /api/tasks/{id}` - Delete task

### Projects
- `GET /api/projects` - List projects
- `GET /api/projects/{id}` - Get project details
- `POST /api/projects` - Create new project
- `PUT /api/projects/{id}` - Update project
- `DELETE /api/projects/{id}` - Delete project

### Users
- `GET /api/users` - List users
- `GET /api/users/{id}` - Get user details
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

## 🔧 Configuration

### Environment Variables
```bash
# Database
ConnectionStrings__DefaultConnection=Host=localhost;Database=taskflow;Username=postgres;Password=password

# JWT Settings
JwtSettings__SecretKey=your-super-secret-key-with-at-least-256-bits
JwtSettings__Issuer=TaskFlow-API
JwtSettings__Audience=TaskFlow-Users
JwtSettings__ExpirationMinutes=60

# Logging
Serilog__MinimumLevel__Default=Information
Serilog__MinimumLevel__Override__Microsoft=Warning
```

### Docker Environment
The `docker-compose.yml` includes:
- **TaskFlow API** - .NET 8 Web API
- **PostgreSQL** - Primary database
- **Redis** - Caching and session storage
- **Seq** - Log aggregation and analysis

## 📝 API Response Format

All API responses follow the envelope pattern:

```json
{
  "success": true,
  "data": {
    "id": "task_001",
    "title": "Implement user authentication",
    "status": "in_progress"
  },
  "message": "Task retrieved successfully",
  "timestamp": "2024-01-15T10:30:00Z",
  "version": "1.0.0"
}
```

Error responses:
```json
{
  "success": false,
  "error": "TASK_NOT_FOUND",
  "message": "Task with ID task_999 not found",
  "timestamp": "2024-01-15T10:30:00Z",
  "version": "1.0.0"
}
```

## 🧪 Testing

### Running Tests
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/TaskFlow.Domain.Tests/
```

### Test Structure
- **Unit Tests** - Domain logic and application services
- **Integration Tests** - API endpoints and database operations
- **Contract Tests** - API response format validation

## 📚 Documentation

- **API Documentation** - Available at `/swagger`
- **Health Checks** - Available at `/health`
- **Metrics** - Available at `/metrics` (Prometheus format)

## 🔍 Monitoring

### Health Checks
- Database connectivity
- Redis connectivity
- External service dependencies
- Custom business logic health

### Logging
- Structured JSON logging
- Correlation IDs for request tracking
- Performance metrics
- Security events

### Metrics
- Request duration
- Error rates
- Database query performance
- Custom business metrics

## 🚀 Deployment

### Docker Production
```bash
# Build production image
docker build -t taskflow-api:latest .

# Run with production settings
docker run -p 5000:5000 taskflow-api:latest
```

### Environment-Specific Configurations
- **Development** - Local database, detailed logging
- **Staging** - Staging database, moderate logging
- **Production** - Production database, minimal logging

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For questions or issues:
- Create an issue in the repository
- Check the API documentation at `/swagger`
- Review the health checks at `/health`

---

**Built with ❤️ using .NET 8, Domain-Driven Design, and modern API best practices.** 