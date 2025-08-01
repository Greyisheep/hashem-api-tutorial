# Zero-Downtime Deployment Guide

## 🎯 Overview

This guide covers strategies for deploying applications with minimal or zero downtime. You'll learn blue-green deployments, rolling updates, health checks, and automated rollback procedures.

## 📋 Prerequisites

- Docker containers ready for deployment
- Load balancer or reverse proxy
- Health check endpoints implemented
- Monitoring and alerting setup

---

## 🔄 Part 1: Deployment Strategies

### Blue-Green Deployment

Blue-green deployment maintains two identical production environments. Only one environment serves traffic at a time.

#### Implementation Steps

1. **Environment Setup**
```bash
# Blue environment (current production)
docker run -d --name blue-app -p 8080:8080 your-app:v1

# Green environment (new version)
docker run -d --name green-app -p 8081:8080 your-app:v2
```

2. **Load Balancer Configuration**
```nginx
# Nginx configuration
upstream app_servers {
    server 127.0.0.1:8080 weight=1;  # Blue (current)
    server 127.0.0.1:8081 weight=0;  # Green (standby)
}

server {
    listen 80;
    server_name your-app.com;
    
    location / {
        proxy_pass http://app_servers;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

3. **Deployment Script**
```bash
#!/bin/bash
# blue-green-deploy.sh

set -e

# Deploy new version to green environment
echo "Deploying new version to green environment..."
docker pull your-app:latest
docker stop green-app || true
docker rm green-app || true
docker run -d --name green-app -p 8081:8080 your-app:latest

# Wait for green environment to be healthy
echo "Waiting for green environment to be healthy..."
for i in {1..30}; do
    if curl -f http://localhost:8081/health; then
        echo "Green environment is healthy"
        break
    fi
    sleep 2
done

# Switch traffic to green environment
echo "Switching traffic to green environment..."
# Update nginx configuration to point to green
sed -i 's/weight=1/weight=0/' /etc/nginx/upstream.conf
sed -i 's/weight=0/weight=1/' /etc/nginx/upstream.conf
nginx -s reload

# Wait for old connections to drain
sleep 30

# Stop blue environment
echo "Stopping blue environment..."
docker stop blue-app
docker rm blue-app

echo "Deployment completed successfully"
```

### Rolling Updates

Rolling updates gradually replace instances while maintaining service availability.

#### Docker Swarm Implementation

```yaml
# docker-compose.yml
version: '3.8'

services:
  taskflow-api:
    image: your-registry/taskflow-api:latest
    deploy:
      replicas: 3
      update_config:
        parallelism: 1
        delay: 10s
        order: start-first
        failure_action: rollback
        monitor: 30s
        max_failure_ratio: 0.3
      restart_policy:
        condition: on-failure
        delay: 5s
        max_attempts: 3
        window: 120s
    ports:
      - "8080:8080"
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
```

#### Kubernetes Implementation

```yaml
# deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: taskflow-api
spec:
  replicas: 3
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 0
  selector:
    matchLabels:
      app: taskflow-api
  template:
    metadata:
      labels:
        app: taskflow-api
    spec:
      containers:
      - name: taskflow-api
        image: your-registry/taskflow-api:latest
        ports:
        - containerPort: 8080
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
```

---

## 🏥 Part 2: Health Checks

### Application Health Check Implementation

```csharp
// HealthController.cs
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;
    private readonly IDbContext _dbContext;
    private readonly IRedisService _redisService;

    public HealthController(
        ILogger<HealthController> logger,
        IDbContext dbContext,
        IRedisService redisService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _redisService = redisService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var healthChecks = new List<HealthCheckResult>();

        // Database health check
        try
        {
            await _dbContext.Database.CanConnectAsync();
            healthChecks.Add(new HealthCheckResult("Database", "Healthy"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            healthChecks.Add(new HealthCheckResult("Database", "Unhealthy", ex.Message));
        }

        // Redis health check
        try
        {
            await _redisService.PingAsync();
            healthChecks.Add(new HealthCheckResult("Redis", "Healthy"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed");
            healthChecks.Add(new HealthCheckResult("Redis", "Unhealthy", ex.Message));
        }

        // Application health check
        healthChecks.Add(new HealthCheckResult("Application", "Healthy"));

        var isHealthy = healthChecks.All(h => h.Status == "Healthy");
        var statusCode = isHealthy ? 200 : 503;

        return StatusCode(statusCode, new
        {
            Status = isHealthy ? "Healthy" : "Unhealthy",
            Timestamp = DateTime.UtcNow,
            Checks = healthChecks
        });
    }
}

public class HealthCheckResult
{
    public string Name { get; set; }
    public string Status { get; set; }
    public string? Error { get; set; }

    public HealthCheckResult(string name, string status, string? error = null)
    {
        Name = name;
        Status = status;
        Error = error;
    }
}
```

### Docker Health Check

```dockerfile
# Dockerfile with health check
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app
COPY . .

EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "TaskFlow.API.dll"]
```

---

## 🔄 Part 3: Rollback Strategies

### Automated Rollback Implementation

```bash
#!/bin/bash
# rollback.sh

set -e

DEPLOYMENT_ID=$1
PREVIOUS_VERSION=$2

if [ -z "$DEPLOYMENT_ID" ] || [ -z "$PREVIOUS_VERSION" ]; then
    echo "Usage: ./rollback.sh <deployment-id> <previous-version>"
    exit 1
fi

echo "Starting rollback to version $PREVIOUS_VERSION..."

# Stop current deployment
docker stop app-current || true
docker rm app-current || true

# Start previous version
docker run -d --name app-current -p 8080:8080 your-app:$PREVIOUS_VERSION

# Wait for health check
echo "Waiting for rollback to be healthy..."
for i in {1..30}; do
    if curl -f http://localhost:8080/health; then
        echo "Rollback successful"
        break
    fi
    sleep 2
done

# Update load balancer if needed
echo "Rollback completed successfully"
```

### Kubernetes Rollback

```bash
# Rollback to previous deployment
kubectl rollout undo deployment/taskflow-api

# Check rollback status
kubectl rollout status deployment/taskflow-api

# View deployment history
kubectl rollout history deployment/taskflow-api
```

---

## 📊 Part 4: Monitoring and Alerting

### Prometheus Metrics

```csharp
// Metrics configuration
public class MetricsConfig
{
    public static void ConfigureMetrics(IServiceCollection services)
    {
        services.AddMetrics();
        
        // Configure Prometheus metrics
        services.AddPrometheusMetrics();
    }
}

// Custom metrics
public class DeploymentMetrics
{
    private readonly Counter _deploymentCounter;
    private readonly Histogram _deploymentDuration;

    public DeploymentMetrics(IMetricsFactory metricsFactory)
    {
        _deploymentCounter = metricsFactory.CreateCounter("deployments_total", "Total number of deployments");
        _deploymentDuration = metricsFactory.CreateHistogram("deployment_duration_seconds", "Deployment duration in seconds");
    }

    public void RecordDeployment(string environment, bool success)
    {
        _deploymentCounter.Add(1, new KeyValuePair<string, object>("environment", environment));
        _deploymentCounter.Add(1, new KeyValuePair<string, object>("success", success.ToString()));
    }

    public IDisposable MeasureDeployment()
    {
        return _deploymentDuration.NewTimer();
    }
}
```

### Grafana Dashboard

```json
{
  "dashboard": {
    "title": "Deployment Monitoring",
    "panels": [
      {
        "title": "Deployment Success Rate",
        "type": "stat",
        "targets": [
          {
            "expr": "rate(deployments_total{success=\"true\"}[5m]) / rate(deployments_total[5m]) * 100"
          }
        ]
      },
      {
        "title": "Deployment Duration",
        "type": "graph",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, deployment_duration_seconds_bucket)"
          }
        ]
      },
      {
        "title": "Application Health",
        "type": "stat",
        "targets": [
          {
            "expr": "up{job=\"taskflow-api\"}"
          }
        ]
      }
    ]
  }
}
```

---

## 🚀 Part 5: Complete Deployment Pipeline

### GitHub Actions with Zero-Downtime Deployment

```yaml
name: Zero-Downtime Deployment

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Build and push Docker image
      uses: docker/build-push-action@v5
      with:
        context: .
        push: true
        tags: |
          your-registry/taskflow-api:${{ github.sha }}
          your-registry/taskflow-api:latest
          
    - name: Deploy to staging
      run: |
        # Deploy to staging environment
        docker-compose -f docker-compose.staging.yml up -d
        
        # Wait for health check
        for i in {1..30}; do
          if curl -f http://staging.your-app.com/health; then
            echo "Staging deployment successful"
            break
          fi
          sleep 2
        done
        
    - name: Run integration tests
      run: |
        # Run tests against staging
        dotnet test --filter Category=Integration
        
    - name: Deploy to production
      run: |
        # Blue-green deployment to production
        ./scripts/blue-green-deploy.sh
        
    - name: Verify deployment
      run: |
        # Verify production deployment
        for i in {1..30}; do
          if curl -f https://your-app.com/health; then
            echo "Production deployment successful"
            break
          fi
          sleep 2
        done
        
    - name: Rollback on failure
      if: failure()
      run: |
        echo "Deployment failed, initiating rollback..."
        ./scripts/rollback.sh ${{ github.sha }} ${{ env.PREVIOUS_VERSION }}
```

---

## 🔧 Part 6: Advanced Deployment Patterns

### Canary Deployment

```yaml
# Canary deployment with traffic splitting
apiVersion: networking.k8s.io/v1
kind: VirtualService
metadata:
  name: taskflow-api
spec:
  hosts:
  - taskflow-api.com
  http:
  - route:
    - destination:
        host: taskflow-api
        subset: stable
      weight: 90
    - destination:
        host: taskflow-api
        subset: canary
      weight: 10
```

### Feature Flag Deployment

```csharp
// Feature flag implementation
public class FeatureFlags
{
    public static bool IsNewFeatureEnabled(string userId)
    {
        // Implement feature flag logic
        // Could be based on user ID, percentage, or other criteria
        return userId.GetHashCode() % 100 < 10; // 10% rollout
    }
}

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTasks()
    {
        var userId = User.Identity?.Name ?? "anonymous";
        
        if (FeatureFlags.IsNewFeatureEnabled(userId))
        {
            // New implementation
            return await GetTasksNew();
        }
        else
        {
            // Old implementation
            return await GetTasksOld();
        }
    }
}
```

---

## 📋 Part 7: Deployment Checklist

### Pre-Deployment Checklist
- [ ] All tests passing
- [ ] Security scan completed
- [ ] Performance tests passed
- [ ] Database migrations ready
- [ ] Rollback plan prepared
- [ ] Monitoring alerts configured
- [ ] Team notified of deployment

### Deployment Checklist
- [ ] Backup current version
- [ ] Deploy to staging first
- [ ] Run integration tests
- [ ] Verify health checks
- [ ] Deploy to production
- [ ] Monitor application health
- [ ] Verify all endpoints
- [ ] Check performance metrics

### Post-Deployment Checklist
- [ ] Monitor error rates
- [ ] Check performance metrics
- [ ] Verify user experience
- [ ] Update documentation
- [ ] Clean up old versions
- [ ] Update deployment logs

---

## 🎯 Key Takeaways

### What You've Learned
1. **Blue-Green Deployment**: Zero-downtime deployment strategy
2. **Rolling Updates**: Gradual replacement of instances
3. **Health Checks**: Comprehensive application monitoring
4. **Rollback Strategies**: Automated failure recovery
5. **Monitoring**: Real-time deployment tracking

### Best Practices
1. **Always test in staging first**
2. **Implement comprehensive health checks**
3. **Monitor deployment metrics**
4. **Have automated rollback procedures**
5. **Use feature flags for gradual rollouts**

### Next Steps
1. **Implement health check endpoints** in your applications
2. **Set up monitoring and alerting**
3. **Create deployment scripts** for your environment
4. **Test rollback procedures** regularly
5. **Document deployment procedures** for your team

This guide provides the foundation for implementing zero-downtime deployments. The key is to always have a backup plan and comprehensive monitoring to ensure smooth deployments. 