# Container Registry Integration Guide

## 🎯 Overview

This guide covers container registry integration for your Docker images. You'll learn how to use Docker Hub, Azure Container Registry, implement security scanning, and automate image pushing.

## 📋 Prerequisites

- Docker images built and ready
- Docker Hub account (or Azure Container Registry)
- GitHub Actions setup
- Security scanning tools

---

## 🐳 Part 1: Docker Hub Integration

### Step 1: Docker Hub Account Setup

1. **Create Docker Hub Account**
   - Go to [Docker Hub](https://hub.docker.com/)
   - Sign up for a free account
   - Create a repository for your application

2. **Generate Access Token**
   - Go to Account Settings → Security
   - Create a new access token
   - Save the token securely

### Step 2: GitHub Secrets Configuration

Add these secrets to your GitHub repository:

```bash
DOCKER_USERNAME=your-docker-username
DOCKER_PASSWORD=your-access-token
```

### Step 3: GitHub Actions Workflow

```yaml
name: Build and Push to Docker Hub

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

env:
  REGISTRY: docker.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  build-and-push:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Log in to Docker Hub
      uses: docker/login-action@v3
      with:
        username: ${{ secrets.DOCKER_USERNAME }}
        password: ${{ secrets.DOCKER_PASSWORD }}
        
    - name: Extract metadata
      id: meta
      uses: docker/metadata-action@v5
      with:
        images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}
        tags: |
          type=ref,event=branch
          type=ref,event=pr
          type=semver,pattern={{version}}
          type=semver,pattern={{major}}.{{minor}}
          type=sha,prefix={{branch}}-
          
    - name: Build and push Docker image
      uses: docker/build-push-action@v5
      with:
        context: .
        push: true
        tags: ${{ steps.meta.outputs.tags }}
        labels: ${{ steps.meta.outputs.labels }}
        cache-from: type=gha
        cache-to: type=gha,mode=max
```

---

## ☁️ Part 2: Azure Container Registry

### Step 1: Create Azure Container Registry

```bash
# Create resource group
az group create --name myResourceGroup --location eastus

# Create container registry
az acr create --resource-group myResourceGroup \
    --name myContainerRegistry --sku Basic

# Enable admin user
az acr update -n myContainerRegistry --admin-enabled true

# Get credentials
az acr credential show --name myContainerRegistry
```

### Step 2: GitHub Actions with Azure Container Registry

```yaml
name: Build and Push to Azure Container Registry

on:
  push:
    branches: [ main ]

env:
  REGISTRY: myContainerRegistry.azurecr.io
  IMAGE_NAME: taskflow-api

jobs:
  build-and-push:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Log in to Azure Container Registry
      uses: azure/docker-login@v1
      with:
        login-server: ${{ secrets.ACR_LOGIN_SERVER }}
        username: ${{ secrets.ACR_USERNAME }}
        password: ${{ secrets.ACR_PASSWORD }}
        
    - name: Build and push Docker image
      uses: docker/build-push-action@v5
      with:
        context: .
        push: true
        tags: |
          ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ github.sha }}
          ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:latest
        cache-from: type=gha
        cache-to: type=gha,mode=max
```

### Step 3: Azure Container Registry Secrets

Add these secrets to your GitHub repository:

```bash
ACR_LOGIN_SERVER=myContainerRegistry.azurecr.io
ACR_USERNAME=myContainerRegistry
ACR_PASSWORD=your-access-token
```

---

## 🔒 Part 3: Security Scanning

### Step 1: Trivy Vulnerability Scanner

```yaml
name: Security Scan

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  security-scan:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Build Docker image
      run: docker build -t my-app:${{ github.sha }} .
      
    - name: Run Trivy vulnerability scanner
      uses: aquasecurity/trivy-action@master
      with:
        image-ref: 'my-app:${{ github.sha }}'
        format: 'sarif'
        output: 'trivy-results.sarif'
        
    - name: Upload Trivy scan results to GitHub Security tab
      uses: github/codeql-action/upload-sarif@v3
      if: always()
      with:
        sarif_file: 'trivy-results.sarif'
```

### Step 2: Snyk Security Scanning

```yaml
    - name: Run Snyk to check for vulnerabilities
      uses: snyk/actions/dotnet@master
      env:
        SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}
      with:
        args: --severity-threshold=high
```

### Step 3: Container Image Signing

```yaml
    - name: Sign Docker image
      uses: docker/build-push-action@v5
      with:
        context: .
        push: true
        tags: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ github.sha }}
        platforms: linux/amd64,linux/arm64
        provenance: true
        sbom: true
```

---

## 🔧 Part 4: Multi-Platform Builds

### Step 1: Multi-Architecture Images

```yaml
name: Multi-Platform Build

on:
  push:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Set up QEMU
      uses: docker/setup-qemu-action@v3
      
    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v3
      
    - name: Log in to Docker Hub
      uses: docker/login-action@v3
      with:
        username: ${{ secrets.DOCKER_USERNAME }}
        password: ${{ secrets.DOCKER_PASSWORD }}
        
    - name: Build and push
      uses: docker/build-push-action@v5
      with:
        context: .
        platforms: linux/amd64,linux/arm64
        push: true
        tags: |
          ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ github.sha }}
          ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:latest
```

---

## 📊 Part 5: Registry Management

### Step 1: Image Tagging Strategy

```bash
# Semantic versioning
docker tag my-app:latest my-registry/my-app:v1.2.3

# Git commit SHA
docker tag my-app:latest my-registry/my-app:sha-abc123

# Environment tags
docker tag my-app:latest my-registry/my-app:staging
docker tag my-app:latest my-registry/my-app:production

# Date tags
docker tag my-app:latest my-registry/my-app:2024-01-15
```

### Step 2: Image Cleanup

```yaml
name: Cleanup Old Images

on:
  schedule:
    - cron: '0 2 * * 0'  # Weekly cleanup

jobs:
  cleanup:
    runs-on: ubuntu-latest
    
    steps:
    - name: Cleanup Docker Hub images
      uses: actions/delete-package-versions@v4
      with:
        package-name: my-app
        min-versions-to-keep: 10
        token: ${{ secrets.GITHUB_TOKEN }}
```

### Step 3: Registry Monitoring

```yaml
name: Registry Health Check

on:
  schedule:
    - cron: '0 */6 * * *'  # Every 6 hours

jobs:
  health-check:
    runs-on: ubuntu-latest
    
    steps:
    - name: Check registry connectivity
      run: |
        # Test Docker Hub connectivity
        docker pull hello-world:latest
        
        # Test Azure Container Registry
        az acr repository list --name myContainerRegistry
        
    - name: Check image vulnerabilities
      run: |
        # Scan latest images for vulnerabilities
        trivy image my-registry/my-app:latest
```

---

## 🚀 Part 6: Complete Registry Pipeline

### Complete GitHub Actions Workflow

```yaml
name: Complete Container Registry Pipeline

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

env:
  REGISTRY: docker.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
        
    - name: Build application
      run: dotnet build --configuration Release
      
    - name: Run tests
      run: dotnet test --no-build --verbosity normal
      
    - name: Build Docker image
      run: docker build -t $IMAGE_NAME:${{ github.sha }} .

  security-scan:
    runs-on: ubuntu-latest
    needs: build-and-test
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Run Trivy vulnerability scanner
      uses: aquasecurity/trivy-action@master
      with:
        image-ref: '${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ github.sha }}'
        format: 'sarif'
        output: 'trivy-results.sarif'
        
    - name: Upload Trivy scan results
      uses: github/codeql-action/upload-sarif@v3
      if: always()
      with:
        sarif_file: 'trivy-results.sarif'

  push-to-registry:
    runs-on: ubuntu-latest
    needs: [build-and-test, security-scan]
    if: github.ref == 'refs/heads/main'
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Set up QEMU
      uses: docker/setup-qemu-action@v3
      
    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v3
      
    - name: Log in to Docker Hub
      uses: docker/login-action@v3
      with:
        username: ${{ secrets.DOCKER_USERNAME }}
        password: ${{ secrets.DOCKER_PASSWORD }}
        
    - name: Extract metadata
      id: meta
      uses: docker/metadata-action@v5
      with:
        images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}
        tags: |
          type=ref,event=branch
          type=semver,pattern={{version}}
          type=sha,prefix={{branch}}-
          
    - name: Build and push Docker image
      uses: docker/build-push-action@v5
      with:
        context: .
        platforms: linux/amd64,linux/arm64
        push: true
        tags: ${{ steps.meta.outputs.tags }}
        labels: ${{ steps.meta.outputs.labels }}
        provenance: true
        sbom: true
        cache-from: type=gha
        cache-to: type=gha,mode=max

  deploy:
    runs-on: ubuntu-latest
    needs: push-to-registry
    if: github.ref == 'refs/heads/main'
    
    steps:
    - name: Deploy to production
      run: |
        # Pull and deploy the new image
        docker pull ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ github.sha }}
        docker-compose up -d
        
    - name: Health check
      run: |
        # Wait for deployment to complete
        sleep 30
        
        # Check if application is healthy
        curl -f http://localhost:8080/health || exit 1
```

---

## 📋 Part 7: Registry Best Practices

### Image Optimization

```dockerfile
# Multi-stage build for smaller images
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TaskFlow.API/TaskFlow.API.csproj", "TaskFlow.API/"]
RUN dotnet restore "TaskFlow.API/TaskFlow.API.csproj"
COPY . .
RUN dotnet build "TaskFlow.API/TaskFlow.API.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS publish
WORKDIR /app
COPY --from=build /app/build .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TaskFlow.API.dll"]
```

### Security Best Practices

```yaml
# .dockerignore file
node_modules
npm-debug.log
.git
.gitignore
README.md
.env
.nyc_output
coverage
.coverage
.pytest_cache
.mypy_cache
.hypothesis
```

### Registry Configuration

```bash
# Configure Docker to use specific registry
docker login myContainerRegistry.azurecr.io

# Set up image scanning
docker scan my-app:latest

# Configure registry policies
az acr update --name myContainerRegistry \
    --admin-enabled false \
    --public-network-access Disabled
```

---

## 🎯 Key Takeaways

### What You've Learned
1. **Docker Hub Integration**: Public registry for container images
2. **Azure Container Registry**: Private registry with enterprise features
3. **Security Scanning**: Vulnerability detection and reporting
4. **Multi-Platform Builds**: Support for different architectures
5. **Registry Management**: Image tagging and cleanup strategies

### Best Practices
1. **Always scan images** for vulnerabilities before pushing
2. **Use multi-stage builds** to reduce image size
3. **Implement proper tagging** strategy for version management
4. **Set up automated cleanup** to manage storage costs
5. **Monitor registry health** and connectivity

### Next Steps
1. **Set up container registry** for your applications
2. **Implement security scanning** in your CI/CD pipeline
3. **Configure multi-platform builds** for better compatibility
4. **Set up automated cleanup** procedures
5. **Monitor registry usage** and costs

This guide provides a complete container registry integration that you can adapt for your specific needs. The key is to implement security scanning and proper image management practices. 