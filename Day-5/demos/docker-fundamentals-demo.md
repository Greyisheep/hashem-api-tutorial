# Docker Fundamentals Demo

## 🎯 Demo Objectives

This demo will show you how to containerize your legacy WCF application and modern .NET applications using Docker. You'll learn multi-stage builds, optimization techniques, and best practices for production deployment.

## 📋 Prerequisites

- Docker Desktop installed
- .NET 8 SDK installed
- Visual Studio 2022 or VS Code
- Sample WCF application (or we'll create one)

---

## 🐳 Part 1: Understanding Docker Basics

### What is Docker?

Docker is a platform for developing, shipping, and running applications in containers. Containers are lightweight, isolated environments that package everything needed to run an application.

### Why Docker for Your Legacy Application?

1. **Consistency**: Same environment across development, testing, and production
2. **Isolation**: No conflicts between different application versions
3. **Portability**: Run anywhere Docker is installed
4. **Scalability**: Easy to scale horizontally
5. **Modernization**: Step towards microservices architecture

---

## 🔧 Part 2: Containerizing Your WCF Application

### Step 1: Create a Basic Dockerfile

Let's start with a simple Dockerfile for your WCF application:

```dockerfile
# Use Windows Server Core as base image for WCF
FROM mcr.microsoft.com/dotnet/framework/wcf:4.8-windowsservercore-ltsc2019

# Set working directory
WORKDIR /app

# Copy application files
COPY . .

# Expose WCF service port
EXPOSE 8080

# Set entry point
ENTRYPOINT ["YourWcfService.exe"]
```

### Step 2: Multi-Stage Build for Modern .NET

For modern .NET applications, we use multi-stage builds to optimize image size:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["TaskFlow.API/TaskFlow.API.csproj", "TaskFlow.API/"]
COPY ["TaskFlow.Application/TaskFlow.Application.csproj", "TaskFlow.Application/"]
COPY ["TaskFlow.Domain/TaskFlow.Domain.csproj", "TaskFlow.Domain/"]
COPY ["TaskFlow.Infrastructure/TaskFlow.Infrastructure.csproj", "TaskFlow.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "TaskFlow.API/TaskFlow.API.csproj"

# Copy source code
COPY . .

# Build application
RUN dotnet build "TaskFlow.API/TaskFlow.API.csproj" -c Release -o /app/build

# Publish stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS publish
WORKDIR /app

# Copy published application
COPY --from=build /app/build .

# Expose port
EXPOSE 80
EXPOSE 443

# Set entry point
ENTRYPOINT ["dotnet", "TaskFlow.API.dll"]
```

### Step 3: Optimized Production Dockerfile

Here's an optimized version with security and performance considerations:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["TaskFlow.API/TaskFlow.API.csproj", "TaskFlow.API/"]
COPY ["TaskFlow.Application/TaskFlow.Application.csproj", "TaskFlow.Application/"]
COPY ["TaskFlow.Domain/TaskFlow.Domain.csproj", "TaskFlow.Domain/"]
COPY ["TaskFlow.Infrastructure/TaskFlow.Infrastructure.csproj", "TaskFlow.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "TaskFlow.API/TaskFlow.API.csproj"

# Copy source code
COPY . .

# Build application
RUN dotnet build "TaskFlow.API/TaskFlow.API.csproj" -c Release -o /app/build

# Publish stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS publish
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Copy published application
COPY --from=build /app/build .

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set entry point
ENTRYPOINT ["dotnet", "TaskFlow.API.dll"]
```

---

## 🚀 Part 3: Docker Compose for Development

### Create docker-compose.yml

```yaml
version: '3.8'

services:
  taskflow-api:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=db;Database=TaskFlow;User Id=sa;Password=YourStrong@Passw0rd;
    depends_on:
      - db
    networks:
      - taskflow-network

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    networks:
      - taskflow-network

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    networks:
      - taskflow-network

volumes:
  sqlserver_data:

networks:
  taskflow-network:
    driver: bridge
```

---

## 🔍 Part 4: Docker Best Practices

### 1. Image Optimization

```dockerfile
# Use specific base image versions
FROM mcr.microsoft.com/dotnet/aspnet:8.0.0

# Use multi-stage builds to reduce image size
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# ... build steps
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
# ... runtime steps

# Remove unnecessary files
RUN rm -rf /var/lib/apt/lists/*

# Use .dockerignore to exclude unnecessary files
```

### 2. Security Best Practices

```dockerfile
# Create non-root user
RUN adduser --disabled-password --gecos "" appuser
USER appuser

# Use specific base images
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Scan for vulnerabilities
# docker run --rm -v /var/run/docker.sock:/var/run/docker.sock \
#   aquasec/trivy image your-image:tag
```

### 3. Health Checks

```dockerfile
# Add health check endpoint to your API
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }
}

# Dockerfile health check
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1
```

---

## 🧪 Part 5: Hands-On Exercise

### Exercise 1: Containerize Your WCF Application

1. **Create a simple WCF service** (if you don't have one):

```csharp
// Simple WCF Service
[ServiceContract]
public interface ICalculatorService
{
    [OperationContract]
    int Add(int a, int b);
}

public class CalculatorService : ICalculatorService
{
    public int Add(int a, int b)
    {
        return a + b;
    }
}
```

2. **Create Dockerfile for WCF**:

```dockerfile
FROM mcr.microsoft.com/dotnet/framework/wcf:4.8-windowsservercore-ltsc2019
WORKDIR /app
COPY . .
EXPOSE 8080
ENTRYPOINT ["CalculatorService.exe"]
```

3. **Build and run**:

```bash
# Build the image
docker build -t wcf-calculator .

# Run the container
docker run -p 8080:8080 wcf-calculator
```

### Exercise 2: Containerize TaskFlow API

1. **Navigate to TaskFlow project**:

```bash
cd taskflow-api-dotnet
```

2. **Create optimized Dockerfile**:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/TaskFlow.API/TaskFlow.API.csproj", "src/TaskFlow.API/"]
COPY ["src/TaskFlow.Application/TaskFlow.Application.csproj", "src/TaskFlow.Application/"]
COPY ["src/TaskFlow.Domain/TaskFlow.Domain.csproj", "src/TaskFlow.Domain/"]
COPY ["src/TaskFlow.Infrastructure/TaskFlow.Infrastructure.csproj", "src/TaskFlow.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/TaskFlow.API/TaskFlow.API.csproj"

# Copy source code
COPY . .

# Build application
RUN dotnet build "src/TaskFlow.API/TaskFlow.API.csproj" -c Release -o /app/build

# Publish stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS publish
WORKDIR /app

# Copy published application
COPY --from=build /app/build .

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set entry point
ENTRYPOINT ["dotnet", "TaskFlow.API.dll"]
```

3. **Build and test**:

```bash
# Build the image
docker build -t taskflow-api .

# Run the container
docker run -p 8080:8080 taskflow-api

# Test the API
curl http://localhost:8080/api/tasks
```

---

## 📊 Part 6: Docker Commands Reference

### Basic Commands

```bash
# Build image
docker build -t your-app .

# Run container
docker run -p 8080:8080 your-app

# List images
docker images

# List containers
docker ps -a

# Stop container
docker stop <container-id>

# Remove container
docker rm <container-id>

# Remove image
docker rmi <image-id>
```

### Advanced Commands

```bash
# View container logs
docker logs <container-id>

# Execute command in running container
docker exec -it <container-id> /bin/bash

# Inspect container
docker inspect <container-id>

# View container resource usage
docker stats

# Clean up unused resources
docker system prune -a
```

---

## 🔧 Part 7: Docker Compose Commands

```bash
# Start services
docker-compose up -d

# Stop services
docker-compose down

# View logs
docker-compose logs -f

# Rebuild services
docker-compose up --build

# Scale services
docker-compose up --scale taskflow-api=3
```

---

## 🎯 Key Takeaways

### What You've Learned
1. **Docker Fundamentals**: Containers, images, Dockerfiles
2. **Multi-Stage Builds**: Optimize image size and security
3. **Best Practices**: Security, performance, health checks
4. **Docker Compose**: Multi-service development environment
5. **Production Readiness**: Health checks, logging, monitoring

### Next Steps
1. **Containerize your WCF application** using the provided examples
2. **Set up Docker Compose** for development environment
3. **Implement health checks** in your applications
4. **Optimize images** for production deployment
5. **Set up CI/CD** to build and push Docker images

### Benefits for Your Legacy Application
- **Consistent Environment**: Same behavior across all environments
- **Easy Deployment**: Deploy anywhere Docker runs
- **Isolation**: No conflicts with other applications
- **Scalability**: Easy to scale horizontally
- **Modernization Path**: Step towards microservices

---

## 📚 Additional Resources

### Documentation
- [Docker Documentation](https://docs.docker.com/)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)

### Tools
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Docker Compose](https://docs.docker.com/compose/)
- [Trivy Security Scanner](https://aquasecurity.github.io/trivy/)

### Learning Path
1. Master basic Docker commands
2. Understand multi-stage builds
3. Implement Docker Compose
4. Add health checks and monitoring
5. Optimize for production deployment

This demo provides the foundation for containerizing your legacy WCF application and modern .NET applications. The next step is to integrate this with CI/CD pipelines for automated deployment. 