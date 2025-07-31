# Multi-stage build for .NET 8 API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# Install curl for health checks
RUN apt-get update --fix-missing && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/* || \
    (echo "Retrying curl installation..." && \
     apt-get update --fix-missing && \
     apt-get install -y curl && \
     rm -rf /var/lib/apt/lists/*)

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Squad.API.sln", "./"]
COPY ["Squad.API/Squad.API.csproj", "Squad.API/"]
COPY ["Squad.Service/Squad.Service.csproj", "Squad.Service/"]
COPY ["Squad.Models/Squad.Models.csproj", "Squad.Models/"]

# Restore dependencies
RUN dotnet restore "Squad.API.sln"

# Copy source code
COPY . .

# Build the application
RUN dotnet build "Squad.API.sln" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Squad.API.sln" -c Release -o /app/publish /p:UseAppHost=false

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
ENTRYPOINT ["dotnet", "Squad.API.dll"] 