# GitHub Actions CI/CD Implementation Guide

## 🎯 Overview

This guide will help you set up a complete CI/CD pipeline using GitHub Actions for your .NET applications. You'll learn how to automate building, testing, security scanning, and deployment of Docker containers.

## 📋 Prerequisites

- GitHub repository with your code
- Docker Hub account (or Azure Container Registry)
- .NET 8 SDK
- Docker Desktop

---

## 🚀 Part 1: Basic CI/CD Pipeline

### Step 1: Create GitHub Actions Workflow

Create `.github/workflows/ci-cd.yml` in your repository:

```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
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
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore
      
    - name: Test
      run: dotnet test --no-build --verbosity normal
      
    - name: Build Docker image
      run: docker build -t $IMAGE_NAME:${{ github.sha }} .
      
    - name: Run Docker container tests
      run: |
        docker run -d --name test-container -p 8080:8080 $IMAGE_NAME:${{ github.sha }}
        sleep 10
        curl -f http://localhost:8080/health || exit 1
        docker stop test-container
        docker rm test-container
```

### Step 2: Add Security Scanning

```yaml
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
        
    - name: Upload Trivy scan results to GitHub Security tab
      uses: github/codeql-action/upload-sarif@v3
      if: always()
      with:
        sarif_file: 'trivy-results.sarif'
        
    - name: Run CodeQL Analysis
      uses: github/codeql-action/init@v3
      with:
        languages: csharp
        
    - name: Perform CodeQL Analysis
      uses: github/codeql-action/analyze@v3
```

---

## 🔐 Part 2: Container Registry Integration

### Step 1: Docker Hub Integration

```yaml
  push-to-registry:
    runs-on: ubuntu-latest
    needs: [build-and-test, security-scan]
    if: github.ref == 'refs/heads/main'
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Log in to Docker Hub
      uses: docker/login-action@v3
      with:
        username: ${{ secrets.DOCKER_USERNAME }}
        password: ${{ secrets.DOCKER_PASSWORD }}
        
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

### Step 2: Azure Container Registry Integration

```yaml
  push-to-acr:
    runs-on: ubuntu-latest
    needs: [build-and-test, security-scan]
    if: github.ref == 'refs/heads/main'
    
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
          ${{ secrets.ACR_LOGIN_SERVER }}/taskflow-api:${{ github.sha }}
          ${{ secrets.ACR_LOGIN_SERVER }}/taskflow-api:latest
        cache-from: type=gha
        cache-to: type=gha,mode=max
```

---

## 🚀 Part 3: Deployment Pipeline

### Step 1: Production Deployment

```yaml
  deploy-to-production:
    runs-on: ubuntu-latest
    needs: push-to-registry
    if: github.ref == 'refs/heads/main'
    environment: production
    
    steps:
    - name: Deploy to production
      run: |
        # Deploy to your production environment
        # This could be Azure Container Instances, Kubernetes, etc.
        echo "Deploying to production..."
        
    - name: Health check
      run: |
        # Wait for deployment to complete
        sleep 30
        
        # Check if application is healthy
        curl -f https://your-production-url/health || exit 1
        
    - name: Notify deployment status
      if: always()
      run: |
        if [ ${{ job.status }} == 'success' ]; then
          echo "✅ Deployment successful"
        else
          echo "❌ Deployment failed"
        fi
```

### Step 2: Staging Deployment

```yaml
  deploy-to-staging:
    runs-on: ubuntu-latest
    needs: push-to-registry
    if: github.ref == 'refs/heads/develop'
    environment: staging
    
    steps:
    - name: Deploy to staging
      run: |
        # Deploy to staging environment
        echo "Deploying to staging..."
        
    - name: Run integration tests
      run: |
        # Run integration tests against staging
        echo "Running integration tests..."
        
    - name: Performance test
      run: |
        # Run performance tests
        echo "Running performance tests..."
```

---

## 🔧 Part 4: Advanced Features

### Step 1: Multi-Environment Deployment

```yaml
  deploy:
    runs-on: ubuntu-latest
    needs: push-to-registry
    if: github.ref == 'refs/heads/main'
    strategy:
      matrix:
        environment: [staging, production]
    
    steps:
    - name: Deploy to ${{ matrix.environment }}
      run: |
        echo "Deploying to ${{ matrix.environment }}..."
        
    - name: Health check ${{ matrix.environment }}
      run: |
        sleep 30
        curl -f https://${{ matrix.environment }}-url/health || exit 1
```

### Step 2: Automated Testing

```yaml
  test:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
        
    - name: Run unit tests
      run: dotnet test --collect:"XPlat Code Coverage"
      
    - name: Upload coverage reports
      uses: codecov/codecov-action@v3
      with:
        file: ./coverage.cobertura.xml
        flags: unittests
        name: codecov-umbrella
        
    - name: Run integration tests
      run: |
        # Start test database
        docker-compose -f docker-compose.test.yml up -d
        
        # Run integration tests
        dotnet test --filter Category=Integration
        
        # Cleanup
        docker-compose -f docker-compose.test.yml down
```

### Step 3: Performance Testing

```yaml
  performance-test:
    runs-on: ubuntu-latest
    needs: deploy-to-staging
    
    steps:
    - name: Setup k6
      uses: grafana/k6-action@v0.3.0
      with:
        filename: performance-tests/load-test.js
        
    - name: Run performance tests
      run: |
        k6 run performance-tests/load-test.js
```

---

## 📊 Part 5: Monitoring and Notifications

### Step 1: Slack Notifications

```yaml
  notify:
    runs-on: ubuntu-latest
    needs: [deploy-to-production, deploy-to-staging]
    if: always()
    
    steps:
    - name: Notify Slack
      uses: 8398a7/action-slack@v3
      with:
        status: ${{ job.status }}
        channel: '#deployments'
        text: |
          Deployment ${{ job.status }} for ${{ github.repository }}
          Environment: ${{ needs.deploy-to-production.result || needs.deploy-to-staging.result }}
          Commit: ${{ github.sha }}
      env:
        SLACK_WEBHOOK_URL: ${{ secrets.SLACK_WEBHOOK_URL }}
```

### Step 2: Email Notifications

```yaml
  email-notification:
    runs-on: ubuntu-latest
    needs: deploy-to-production
    if: always()
    
    steps:
    - name: Send email notification
      uses: dawidd6/action-send-mail@v3
      with:
        server_address: smtp.gmail.com
        server_port: 587
        username: ${{ secrets.MAIL_USERNAME }}
        password: ${{ secrets.MAIL_PASSWORD }}
        subject: "Deployment ${{ job.status }} - ${{ github.repository }}"
        to: ${{ secrets.NOTIFICATION_EMAIL }}
        from: "CI/CD Pipeline"
        body: |
          Deployment ${{ job.status }} for ${{ github.repository }}
          Environment: Production
          Commit: ${{ github.sha }}
          Branch: ${{ github.ref }}
```

---

## 🔐 Part 6: Security Best Practices

### Step 1: Secrets Management

Create these secrets in your GitHub repository:

```bash
# Docker Hub
DOCKER_USERNAME=your-docker-username
DOCKER_PASSWORD=your-docker-password

# Azure Container Registry
ACR_LOGIN_SERVER=your-registry.azurecr.io
ACR_USERNAME=your-acr-username
ACR_PASSWORD=your-acr-password

# Notifications
SLACK_WEBHOOK_URL=https://hooks.slack.com/services/...
MAIL_USERNAME=your-email@gmail.com
MAIL_PASSWORD=your-app-password
NOTIFICATION_EMAIL=team@yourcompany.com
```

### Step 2: Environment Protection

```yaml
# In your repository settings, create environments:
# - staging
# - production

# Add protection rules:
# - Required reviewers
# - Wait timer
# - Deployment branches
```

---

## 📈 Part 7: Complete Workflow Example

Here's a complete GitHub Actions workflow that combines all features:

```yaml
name: Complete CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
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
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore
      
    - name: Run unit tests
      run: dotnet test --collect:"XPlat Code Coverage"
      
    - name: Upload coverage reports
      uses: codecov/codecov-action@v3
      with:
        file: ./coverage.cobertura.xml
        
    - name: Build Docker image
      run: docker build -t $IMAGE_NAME:${{ github.sha }} .
      
    - name: Run container tests
      run: |
        docker run -d --name test-container -p 8080:8080 $IMAGE_NAME:${{ github.sha }}
        sleep 10
        curl -f http://localhost:8080/health || exit 1
        docker stop test-container
        docker rm test-container

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
        
    - name: Run CodeQL Analysis
      uses: github/codeql-action/init@v3
      with:
        languages: csharp
        
    - name: Perform CodeQL Analysis
      uses: github/codeql-action/analyze@v3

  push-to-registry:
    runs-on: ubuntu-latest
    needs: [build-and-test, security-scan]
    if: github.ref == 'refs/heads/main'
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Log in to Docker Hub
      uses: docker/login-action@v3
      with:
        username: ${{ secrets.DOCKER_USERNAME }}
        password: ${{ secrets.DOCKER_PASSWORD }}
        
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

  deploy-to-staging:
    runs-on: ubuntu-latest
    needs: push-to-registry
    if: github.ref == 'refs/heads/develop'
    environment: staging
    
    steps:
    - name: Deploy to staging
      run: echo "Deploying to staging..."
      
    - name: Run integration tests
      run: echo "Running integration tests..."

  deploy-to-production:
    runs-on: ubuntu-latest
    needs: push-to-registry
    if: github.ref == 'refs/heads/main'
    environment: production
    
    steps:
    - name: Deploy to production
      run: echo "Deploying to production..."
      
    - name: Health check
      run: |
        sleep 30
        curl -f https://your-production-url/health || exit 1

  notify:
    runs-on: ubuntu-latest
    needs: [deploy-to-production, deploy-to-staging]
    if: always()
    
    steps:
    - name: Notify Slack
      uses: 8398a7/action-slack@v3
      with:
        status: ${{ job.status }}
        channel: '#deployments'
        text: |
          Deployment ${{ job.status }} for ${{ github.repository }}
          Environment: ${{ needs.deploy-to-production.result || needs.deploy-to-staging.result }}
          Commit: ${{ github.sha }}
      env:
        SLACK_WEBHOOK_URL: ${{ secrets.SLACK_WEBHOOK_URL }}
```

---

## 🎯 Key Takeaways

### What You've Learned
1. **CI/CD Fundamentals**: Automated build, test, and deployment
2. **Security Integration**: Vulnerability scanning and code analysis
3. **Container Registry**: Docker Hub and Azure Container Registry
4. **Multi-Environment Deployment**: Staging and production environments
5. **Monitoring and Notifications**: Slack and email notifications

### Best Practices
1. **Security First**: Always scan for vulnerabilities
2. **Testing**: Comprehensive unit and integration tests
3. **Monitoring**: Health checks and performance monitoring
4. **Notifications**: Keep team informed of deployment status
5. **Environment Protection**: Use GitHub environments for production

### Next Steps
1. **Set up secrets** in your GitHub repository
2. **Configure environments** for staging and production
3. **Implement health checks** in your applications
4. **Add performance testing** to your pipeline
5. **Set up monitoring** and alerting

This guide provides a complete CI/CD pipeline that you can adapt for your specific needs. The pipeline includes security scanning, automated testing, and multi-environment deployment with proper monitoring and notifications. 