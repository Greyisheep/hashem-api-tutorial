# 🚀 TaskFlow API Setup Guide

This guide will help you set up and run the TaskFlow API with Docker, even if you're not familiar with .NET.

## 📋 Prerequisites

### Required Software
- **Docker Desktop** - Download from [docker.com](https://www.docker.com/products/docker-desktop/)
- **Git** - For cloning the repository
- **A web browser** - For testing the API

### Optional (for development)
- **Visual Studio Code** - For viewing code
- **Postman** - For API testing

## 🔧 Step-by-Step Setup

### 1. **Clone and Navigate to Project**
```bash
# If you haven't already cloned the repository
git clone <repository-url>
cd taskflow-api-dotnet
```

### 2. **Verify Docker is Running**
```bash
# Check if Docker is running
docker --version
docker-compose --version
```

If Docker isn't running, start Docker Desktop and wait for it to fully load.

### 3. **Set Up Secrets (First Time Only)**
```bash
# Run the secure secrets setup script
./setup-secrets.sh
```

### 4. **Start the Application**
```bash
# Option 1: Use the provided script (recommended)
./start.sh

# Option 2: Use docker-compose directly
docker-compose up --build
```

### 5. **Wait for Services to Start**
You'll see output like this:
```
taskflow-api-dotnet_postgres_1    | 2024-01-15 10:30:00.000 UTC [1] LOG:  database system is ready to accept connections
taskflow-api-dotnet_redis_1       | 1:M 15 Jan 10:30:00.000 * Ready to accept connections
taskflow-api-dotnet_taskflow-api_1 | info: Microsoft.Hosting.Lifetime[14]
taskflow-api-dotnet_taskflow-api_1 |       Now listening on: http://[::]:5000
```

### 6. **Test the API**
Once everything is running, you can test the API:

- **API Base URL**: `https://localhost:7001`
- **Health Check**: `https://localhost:7001/health`
- **Swagger Documentation**: `https://localhost:7001/swagger`

## 🔐 Security Features to Test

### OAuth 2.0 with Google
1. **Frontend Testing**: Visit `http://localhost:3000` (if you run the frontend)
2. **Postman Testing**: Import the Postman collection and test OAuth endpoints

### Rate Limiting
- Make multiple requests to see rate limiting in action
- Check response headers for `X-RateLimit-*` values

### Security Headers
- Use browser dev tools to inspect response headers
- Look for security headers like `X-Frame-Options`, `X-Content-Type-Options`

## 🛠️ Understanding the Architecture

### What Each Service Does

| Service | Purpose | Port |
|---------|---------|------|
| **taskflow-api** | Main API application | 7001 |
| **postgres** | Database | 5432 |
| **redis** | Caching | 6379 |
| **seq** | Log aggregation | 5341 |
| **pgadmin** | Database management | 5050 |

### Key Files Explained

| File | Purpose |
|------|---------|
| `docker-compose.yml` | Defines all services and their configuration |
| `Dockerfile` | Instructions for building the API container |
| `start.sh` | Convenience script to start everything |
| `src/TaskFlow.API/Program.cs` | Main application entry point |
| `src/TaskFlow.API/Controllers/` | API endpoints |
| `frontend/index.html` | Simple web interface for testing |

## 🔍 Troubleshooting

### Common Issues

#### 1. **Port Already in Use**
```bash
# Check what's using the port
netstat -an | grep 7001

# Stop the service using the port, or change the port in docker-compose.yml
```

#### 2. **Docker Not Running**
```bash
# Start Docker Desktop
# Wait for it to fully load, then try again
docker-compose up --build
```

#### 3. **Database Connection Issues**
```bash
# Check if PostgreSQL is running
docker-compose ps

# View logs
docker-compose logs postgres
```

#### 4. **Build Errors**
```bash
# Clean and rebuild
docker-compose down
docker-compose build --no-cache
docker-compose up
```

### Useful Commands

```bash
# View running containers
docker-compose ps

# View logs for a specific service
docker-compose logs taskflow-api

# Stop all services
docker-compose down

# Stop and remove volumes (resets database)
docker-compose down -v

# Rebuild and start
docker-compose up --build
```

## 🧪 Testing the API

### 1. **Health Check**
```bash
curl https://localhost:7001/health
```

### 2. **OAuth Flow**
1. Visit `https://localhost:7001/api/auth/google-login`
2. You'll be redirected to Google for authentication
3. After successful login, you'll get a JWT token

### 3. **Rate Limiting Test**
```bash
# Make multiple requests quickly
for i in {1..10}; do
  curl https://localhost:7001/health
  sleep 0.1
done
```

### 4. **Security Headers Test**
```bash
curl -I https://localhost:7001/health
```

## 📊 Monitoring

### Logs
- **API Logs**: `docker-compose logs taskflow-api`
- **Database Logs**: `docker-compose logs postgres`
- **Seq Dashboard**: `http://localhost:5341` (admin/admin123)

### Database Management
- **pgAdmin**: `http://localhost:5050` (admin@taskflow.com/admin123)
- **Direct Connection**: `localhost:5432`

## 🔐 Security Configuration

### Environment Variables
The application uses these environment variables (configured in `.env` file):

- `GOOGLE_CLIENT_ID`: Your Google OAuth client ID
- `GOOGLE_CLIENT_SECRET`: Your Google OAuth client secret
- `JWT_SECRET_KEY`: JWT signing key
- `POSTGRES_PASSWORD`: Database password

### Secrets Management
- ✅ **Secure**: Secrets are stored in `.env` file (not in docker-compose.yml)
- ✅ **Protected**: `.env` is in `.gitignore` to prevent accidental commits
- ✅ **Template**: `env.template` shows the structure without real secrets
- ✅ **Setup Script**: `setup-secrets.sh` creates `.env` with your credentials
- ✅ **Production Ready**: Use proper secrets management in production

### Security Best Practices Implemented
- 🔒 **No hardcoded secrets** in source code
- 🔒 **Environment variables** for all sensitive data
- 🔒 **Template files** for easy setup
- 🔒 **Git protection** via .gitignore
- 🔒 **Secure JWT configuration** with proper signing
- 🔒 **OAuth 2.0 with PKCE** for secure authentication

## 🚀 Production Deployment

For production, you should:

1. **Use proper secrets management** (Azure Key Vault, AWS Secrets Manager, etc.)
2. **Change default passwords** in docker-compose.yml
3. **Use HTTPS certificates**
4. **Configure proper logging**
5. **Set up monitoring and alerting**

## 📚 Learning Resources

### .NET Concepts
- **ASP.NET Core**: Web framework for building APIs
- **Entity Framework**: Database access layer
- **Dependency Injection**: Service management
- **Middleware**: Request/response pipeline

### Security Concepts
- **OAuth 2.0**: Authorization protocol
- **JWT**: JSON Web Tokens for authentication
- **Rate Limiting**: Preventing API abuse
- **Security Headers**: Browser security features

## 🤝 Getting Help

If you encounter issues:

1. **Check the logs**: `docker-compose logs [service-name]`
2. **Verify Docker is running**: `docker info`
3. **Check port availability**: `netstat -an | grep [port]`
4. **Restart services**: `docker-compose restart`

## 🎉 Success Indicators

You'll know everything is working when:

- ✅ Docker containers are running: `docker-compose ps`
- ✅ Health check returns 200: `curl https://localhost:7001/health`
- ✅ Swagger UI loads: `https://localhost:7001/swagger`
- ✅ Google OAuth redirects work
- ✅ Rate limiting headers appear in responses

---

**🎯 Goal**: By the end of this setup, you should have a fully functional, secure API with OAuth authentication, rate limiting, and comprehensive security features running in Docker containers. 