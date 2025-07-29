# TaskFlow API - Quick Setup Guide

## 🚀 Quick Start

### Prerequisites
- Docker Desktop installed and running
- .NET 8 SDK (for local development)

### 1. Start the API with Docker

**Windows:**
```bash
start.bat
```

**Linux/Mac:**
```bash
chmod +x start.sh
./start.sh
```

### 2. Access the Services

Once started, you can access:

- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health
- **Database (pgAdmin)**: http://localhost:5050
- **Logging (Seq)**: http://localhost:5341

### 3. Import Postman Collection

1. Open Postman
2. Import the collection: `postman/TaskFlow-API.postman_collection.json`
3. Import the environment: `postman/TaskFlow-API.postman_environment.json`
4. Select the "TaskFlow API - Development" environment

### 4. Test the API

1. **Health Check**: GET http://localhost:5000/health
2. **Create Task**: POST http://localhost:5000/api/tasks
3. **Get Tasks**: GET http://localhost:5000/api/tasks

## 🏗️ Architecture

```
TaskFlow.API/
├── src/
│   ├── TaskFlow.API/              # Web API layer
│   ├── TaskFlow.Application/      # Application services & DTOs
│   ├── TaskFlow.Domain/           # Domain entities & business logic
│   └── TaskFlow.Infrastructure/   # Data access & external services
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

## 📊 API Response Format

All responses follow the envelope pattern:

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

## 🧪 Testing

### Using Postman
1. Import the collection and environment
2. Run the "Login" request to get an auth token
3. The token will be automatically set for subsequent requests
4. Test all endpoints in the collection

### Using Swagger
1. Open http://localhost:5000/swagger
2. Click "Authorize" and enter your JWT token
3. Test endpoints directly from the UI

## 🔍 Monitoring

### Health Checks
- **Database**: Checks PostgreSQL connectivity
- **Self**: Basic API health check
- **Custom**: Business logic health checks

### Logging
- **Console**: Real-time logs in terminal
- **File**: Daily rotating log files
- **Seq**: Centralized log aggregation

### Metrics
- Request duration
- Error rates
- Database performance
- Custom business metrics

## 🛠️ Development

### Local Development
```bash
# Restore dependencies
dotnet restore

# Run database migrations
dotnet ef database update

# Start the API
dotnet run --project src/TaskFlow.API
```

### Adding New Features
1. Add domain entities in `TaskFlow.Domain`
2. Create DTOs in `TaskFlow.Application`
3. Implement commands/queries with MediatR
4. Add controllers in `TaskFlow.API`
5. Update Postman collection

## 🚀 Production Deployment

### Docker Production
```bash
# Build production image
docker build -t taskflow-api:latest .

# Run with production settings
docker run -p 5000:5000 taskflow-api:latest
```

### Environment Variables
```bash
# Database
ConnectionStrings__DefaultConnection=Host=prod-db;Database=taskflow;Username=prod_user;Password=secure_password

# JWT Settings
JwtSettings__SecretKey=your-production-secret-key-with-at-least-256-bits
JwtSettings__Issuer=TaskFlow-API
JwtSettings__Audience=TaskFlow-Users
JwtSettings__ExpirationMinutes=60
```

## 📚 Documentation

- **API Documentation**: Available at `/swagger`
- **Health Checks**: Available at `/health`
- **Metrics**: Available at `/metrics` (Prometheus format)

## 🆘 Troubleshooting

### Common Issues

1. **Docker not running**
   - Start Docker Desktop
   - Wait for it to fully initialize

2. **Port conflicts**
   - Check if ports 5000, 5432, 5050, 5341 are available
   - Stop conflicting services

3. **Database connection issues**
   - Wait for PostgreSQL to fully start
   - Check connection string in docker-compose.yml

4. **Authentication errors**
   - Ensure JWT settings are configured
   - Check token expiration

### Logs
- Check Docker logs: `docker-compose logs`
- Check API logs: `docker-compose logs taskflow-api`
- Check database logs: `docker-compose logs postgres`

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

---

**Built with ❤️ using .NET 8, Domain-Driven Design, and modern API best practices.** 