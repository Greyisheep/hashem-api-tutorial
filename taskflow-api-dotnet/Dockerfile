# Multi-stage build for .NET 8 API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["TaskFlow.sln", "./"]
COPY ["src/TaskFlow.API/TaskFlow.API.csproj", "src/TaskFlow.API/"]
COPY ["src/TaskFlow.Application/TaskFlow.Application.csproj", "src/TaskFlow.Application/"]
COPY ["src/TaskFlow.Domain/TaskFlow.Domain.csproj", "src/TaskFlow.Domain/"]
COPY ["src/TaskFlow.Infrastructure/TaskFlow.Infrastructure.csproj", "src/TaskFlow.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "TaskFlow.sln"

# Copy source code
COPY . .

# Build the application
RUN dotnet build "TaskFlow.sln" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "TaskFlow.sln" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM base AS final
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published application
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs && chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "TaskFlow.API.dll"] 