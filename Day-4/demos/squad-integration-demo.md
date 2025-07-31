# Squad.co Integration Demo Script

## 🎯 Demo Overview

**Demonstrate a complete production-ready payment gateway integration with Squad.co, including rate limiting, security measures, versioning, and monitoring.**

### 📋 Demo Objectives
- Show Squad.co payment flow end-to-end
- Demonstrate rate limiting in action
- Show security measures implementation
- Display API versioning strategy
- Present monitoring and alerting

## 🚀 Demo Structure

### Phase 1: Setup & Environment (5 minutes)

#### Step 1: Environment Setup
```bash
# Navigate to Squad.API
cd Squad.API

# Set up secrets (script will self-delete)
./setup-secrets.sh

# Start the application
docker-compose up -d

# Verify all services are running
docker-compose ps

# Check application health
curl http://localhost:5000/health
```

#### Step 2: Verify Services
```bash
# Check PostgreSQL
docker exec squad-postgres pg_isready -U squad_user -d squad_db

# Check Redis
docker exec squad-redis redis-cli ping

# Check API logs
docker logs squad-api
```

### Phase 2: Squad.co Payment Flow Demo (15 minutes)

#### Step 1: Initialize Payment
```bash
# Create a payment request
curl -X POST http://localhost:5000/api/v1/payments/initialize \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "amount": 5000,
    "currency": "NGN",
    "email": "customer@example.com",
    "phoneNumber": "+2348012345678",
    "description": "Demo payment for Day 4",
    "callbackUrl": "http://localhost:3000/payment/callback"
  }'
```

**Expected Response**:
```json
{
  "success": true,
  "data": {
    "paymentId": "PAY_123456789",
    "redirectUrl": "https://sandbox-api-d.squadco.com/payment/PAY_123456789",
    "reference": "REF_123456789",
    "amount": 5000,
    "currency": "NGN",
    "status": "PENDING"
  },
  "message": "Payment initialized successfully"
}
```

#### Step 2: Payment Verification
```bash
# Verify payment status
curl -X GET http://localhost:5000/api/v1/payments/verify/PAY_123456789 \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

#### Step 3: Webhook Handling Demo
```bash
# Simulate webhook from Squad.co
curl -X POST http://localhost:5000/api/v1/payments/webhook \
  -H "Content-Type: application/json" \
  -H "X-Squad-Signature: WEBHOOK_SIGNATURE" \
  -d '{
    "event": "payment.successful",
    "data": {
      "paymentId": "PAY_123456789",
      "status": "SUCCESSFUL",
      "amount": 5000,
      "currency": "NGN",
      "reference": "REF_123456789"
    }
  }'
```

### Phase 3: Rate Limiting Demo (10 minutes)

#### Step 1: Normal Usage
```bash
# Make normal requests
for i in {1..5}; do
  curl -X GET http://localhost:5000/api/v1/health
  echo "Request $i completed"
done
```

#### Step 2: Rate Limit Exceeded
```bash
# Exceed rate limit
for i in {1..15}; do
  response=$(curl -s -w "%{http_code}" -X GET http://localhost:5000/api/v1/health)
  echo "Request $i: $response"
  
  if [[ $response == *"429"* ]]; then
    echo "Rate limit exceeded!"
    break
  fi
done
```

#### Step 3: Rate Limit Headers
```bash
# Check rate limit headers
curl -I -X GET http://localhost:5000/api/v1/health
```

**Expected Headers**:
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 99
X-RateLimit-Reset: Wed, 21 Oct 2024 07:28:00 GMT
```

### Phase 4: Security Measures Demo (10 minutes)

#### Step 1: Security Headers
```bash
# Check security headers
curl -I -X GET http://localhost:5000/api/v1/health
```

**Expected Security Headers**:
```
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Strict-Transport-Security: max-age=31536000; includeSubDomains
Content-Security-Policy: default-src 'self'
```

#### Step 2: Input Validation
```bash
# Test invalid input
curl -X POST http://localhost:5000/api/v1/payments/initialize \
  -H "Content-Type: application/json" \
  -d '{
    "amount": -100,
    "currency": "INVALID",
    "email": "invalid-email"
  }'
```

**Expected Response**:
```json
{
  "success": false,
  "errors": [
    "Amount must be greater than 0",
    "Currency must be a valid 3-letter code",
    "Email must be valid"
  ]
}
```

#### Step 3: Authentication Demo
```bash
# Test without authentication
curl -X GET http://localhost:5000/api/v1/payments

# Test with invalid token
curl -X GET http://localhost:5000/api/v1/payments \
  -H "Authorization: Bearer INVALID_TOKEN"
```

### Phase 5: API Versioning Demo (10 minutes)

#### Step 1: V1 Endpoint
```bash
# Use V1 endpoint
curl -X GET http://localhost:5000/api/v1/payments \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Expected V1 Response**:
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "amount": 5000,
      "currency": "NGN",
      "status": "SUCCESSFUL",
      "createdAt": "2024-01-15T10:30:00Z"
    }
  ]
}
```

#### Step 2: V2 Endpoint
```bash
# Use V2 endpoint with enhanced features
curl -X GET http://localhost:5000/api/v2/payments \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Expected V2 Response**:
```json
{
  "success": true,
  "data": {
    "payments": [
      {
        "id": 1,
        "amount": 5000,
        "currency": "NGN",
        "status": "SUCCESSFUL",
        "createdAt": "2024-01-15T10:30:00Z",
        "updatedAt": "2024-01-15T10:35:00Z",
        "metadata": {
          "customerId": "CUST_123",
          "orderId": "ORDER_456"
        }
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 10,
      "total": 1,
      "totalPages": 1
    }
  }
}
```

#### Step 3: Deprecation Warning
```bash
# Check deprecation headers for V1
curl -I -X GET http://localhost:5000/api/v1/payments
```

**Expected Deprecation Headers**:
```
Deprecation: true
Sunset: 2024-12-31
Link: </api/v2/payments>; rel="successor-version"
```

### Phase 6: Monitoring & Alerting Demo (10 minutes)

#### Step 1: Request Monitoring
```bash
# Check application logs
docker logs squad-api --tail 20

# Check rate limiting logs
docker logs squad-api | grep "Rate limit exceeded"
```

#### Step 2: Performance Metrics
```bash
# Check response times
curl -w "@curl-format.txt" -o /dev/null -s http://localhost:5000/api/v1/health
```

**Create curl-format.txt**:
```
     time_namelookup:  %{time_namelookup}\n
        time_connect:  %{time_connect}\n
     time_appconnect:  %{time_appconnect}\n
    time_pretransfer:  %{time_pretransfer}\n
       time_redirect:  %{time_redirect}\n
  time_starttransfer:  %{time_starttransfer}\n
                     ----------\n
          time_total:  %{time_total}\n
```

#### Step 3: Error Tracking
```bash
# Generate some errors
curl -X POST http://localhost:5000/api/v1/payments/initialize \
  -H "Content-Type: application/json" \
  -d '{"invalid": "data"}'

# Check error logs
docker logs squad-api | grep "ERROR"
```

### Phase 7: Production Features Demo (10 minutes)

#### Step 1: Circuit Breaker Demo
```bash
# Test circuit breaker with Squad.co API
curl -X POST http://localhost:5000/api/v1/payments/initialize \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 5000,
    "currency": "NGN",
    "email": "test@example.com"
  }'
```

#### Step 2: Caching Demo
```bash
# First request (cache miss)
curl -X GET http://localhost:5000/api/v1/payments/PAY_123456789

# Second request (cache hit)
curl -X GET http://localhost:5000/api/v1/payments/PAY_123456789
```

#### Step 3: Health Check
```bash
# Comprehensive health check
curl -X GET http://localhost:5000/api/v1/health
```

**Expected Response**:
```json
{
  "status": "healthy",
  "timestamp": "2024-01-15T10:30:00Z",
  "services": {
    "database": "healthy",
    "redis": "healthy",
    "squadApi": "healthy"
  },
  "metrics": {
    "uptime": "2h 30m",
    "requestsPerMinute": 45,
    "errorRate": 0.02
  }
}
```

## 🎯 Demo Scripts

### Pre-Demo Setup
```bash
#!/bin/bash
# pre-demo-setup.sh

echo "🚀 Setting up Squad.API demo environment..."

# Navigate to Squad.API
cd Squad.API

# Set up environment
./setup-secrets.sh

# Start services
docker-compose up -d

# Wait for services to be ready
echo "⏳ Waiting for services to start..."
sleep 30

# Verify setup
echo "✅ Checking service health..."
curl -f http://localhost:5000/health || exit 1

echo "🎉 Demo environment ready!"
```

### Post-Demo Cleanup
```bash
#!/bin/bash
# post-demo-cleanup.sh

echo "🧹 Cleaning up demo environment..."

# Stop services
docker-compose down

# Remove volumes (optional)
docker-compose down -v

# Remove .env file
rm -f .env

echo "✅ Cleanup complete!"
```

## 📊 Demo Metrics

### Success Criteria
- ✅ All services start successfully
- ✅ Payment flow works end-to-end
- ✅ Rate limiting functions properly
- ✅ Security headers are present
- ✅ API versioning works correctly
- ✅ Monitoring shows expected data
- ✅ Error handling works as expected

### Demo Checklist
- [ ] Environment setup completed
- [ ] Squad.co integration working
- [ ] Rate limiting demonstrated
- [ ] Security measures shown
- [ ] API versioning displayed
- [ ] Monitoring data visible
- [ ] Error scenarios handled
- [ ] Performance metrics collected

## 🎓 Demo Tips

### For Presenters
1. **Prepare Environment**: Test the demo environment before the presentation
2. **Have Backup Plans**: Prepare alternative scenarios if external services fail
3. **Explain Each Step**: Provide context for what's happening
4. **Show Real Data**: Use realistic payment amounts and data
5. **Handle Questions**: Be prepared to explain technical details

### For Students
1. **Follow Along**: Try the commands on your own environment
2. **Ask Questions**: Don't hesitate to ask for clarification
3. **Take Notes**: Document important concepts and commands
4. **Experiment**: Try different scenarios and edge cases
5. **Practice**: Repeat the demo steps after the session

---

**Remember**: This demo showcases production-ready API development. Focus on the real-world application of the concepts learned throughout Day 4! 🚀 