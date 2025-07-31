# Squad.API Setup Guide

## 🚀 Quick Start with Docker

### Prerequisites
- Docker and Docker Compose installed
- Git (for cloning the repository)

### Start the Application
```bash
# Windows
start-docker.bat

# Linux/Mac
chmod +x start-docker.sh
./start-docker.sh
```

## 🔐 Secrets Configuration

### Current Secrets (✅ Already Configured)
Based on your provided information, these secrets are already configured:

```env
# Squad.co API Keys
SQUAD_API_KEY=sandbox_sk_4e0cb820488990016ea2bdd648ca077a13059a626904
SQUAD_PUBLIC_KEY=sandbox_pk_4e0cb820488990016ea2bdd035cf18751b17f9026572

# JWT Configuration
JWT_SECRET=fsdbhgevfugeilqgsfcnagvsuyqlXJ1lorkix_ownQGD
JWT_ISSUER=https://localhost:5001
JWT_AUDIENCE=https://localhost:5001
JWT_EXPIRY=1h

# Legacy Bank API
BANK_API_KEY=sandbox_sk_4e0cb820488990016ea2bdd648ca077a13059a626904

# Encryption
SALT_VALUE=TGfdg542*&dltr51g
PASS_PHRASE=Jk6547fdrloln53@#qj
INIT_VECTOR=Goj0458jgte32vb0
PASSWORD_ITERATIONS=2
BLOCKSIZE=32
```

### Missing Secrets (❌ Need to Configure)

#### 1. **SQUAD_WEBHOOK_SECRET** (Critical)
This is the most important missing secret. You need to:

1. **Generate a secure webhook secret**:
   ```bash
   # Generate a random 32-character secret
   openssl rand -hex 16
   # Or use an online generator
   ```

2. **Add it to your .env file**:
   ```env
   SQUAD_WEBHOOK_SECRET=your_generated_secret_here
   ```

3. **Configure it in Squad.co Dashboard**:
   - Log into your Squad.co dashboard
   - Go to Settings → Webhooks
   - Set the webhook secret to match your generated secret

#### 2. **Database Connection String** (Optional Enhancement)
For production, consider using a more secure database connection:

```env
# For production with SSL
ConnectionStrings__DefaultConnection=Host=your-db-host;Port=5432;Database=squad_db;Username=squad_user;Password=squad_password;SSL Mode=Require;
```

## 🌐 Webhook URL Configuration

### For Development (Local Testing)
```
http://localhost:5000/api/squadpayment/webhook
```

### For Production
```
https://your-domain.com/api/squadpayment/webhook
```

### How to Set Webhook URL in Squad.co Dashboard

1. **Log into Squad.co Dashboard**
2. **Navigate to Settings → Webhooks**
3. **Configure the following**:
   - **Webhook URL**: `https://your-domain.com/api/squadpayment/webhook`
   - **Webhook Secret**: (The secret you generated above)
   - **Events to Listen For**:
     - `payment.success`
     - `payment.failed`
     - `transfer.success`
     - `transfer.failed`

### Testing Webhook Locally
For local development, you can use tools like:
- **ngrok**: `ngrok http 5000`
- **webhook.site**: For testing webhook delivery
- **Postman**: Use the webhook test in the provided collection

## 📊 Database Schema

The application automatically creates these tables:

### `squad_transactions`
- Stores all payment initiation requests
- Tracks transaction status
- Stores customer and payment details

### `squad_webhook_events`
- Stores all incoming webhook events
- Includes signature validation
- Tracks processing status

### `squad_payment_verifications`
- Stores manual verification requests
- Caches verification responses
- Prevents duplicate verifications

## 🔍 Testing the Setup

### 1. Health Check
```bash
curl http://localhost:5000/health
```

### 2. Swagger Documentation
```
http://localhost:5000/swagger
```

### 3. Test Payment Initiation
```bash
curl -X POST http://localhost:5000/api/squadpayment/initiate \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "email": "test@example.com",
    "amount": 10000,
    "currency": "NGN",
    "customer_name": "Test User"
  }'
```

### 4. Database Verification
```bash
# Connect to PostgreSQL
docker exec -it squad-postgres psql -U squad_user -d squad_db

# Check tables
\dt

# Check sample data
SELECT * FROM squad_transactions;
```

## 🚨 Security Checklist

- [ ] Webhook secret is generated and configured
- [ ] JWT secret is strong and unique
- [ ] Database passwords are secure
- [ ] HTTPS is enabled for production
- [ ] Webhook signature validation is working
- [ ] Rate limiting is configured
- [ ] CORS is properly configured

## 🐛 Troubleshooting

### Common Issues

1. **Docker Build Fails**
   ```bash
   # Clean and rebuild
   docker-compose down
   docker system prune -f
   docker-compose up --build
   ```

2. **Database Connection Issues**
   ```bash
   # Check if PostgreSQL is running
   docker-compose ps
   
   # Check logs
   docker-compose logs postgres
   ```

3. **Webhook Not Receiving Events**
   - Verify webhook URL is accessible
   - Check webhook secret matches
   - Ensure HTTPS is used in production
   - Check firewall settings

4. **Squad.co API Errors**
   - Verify API key is correct
   - Check sandbox vs production URLs
   - Ensure proper authorization headers

## 📝 Environment Variables Summary

### Required for Squad.co Integration
```env
SQUAD_API_KEY=sandbox_sk_4e0cb820488990016ea2bdd648ca077a13059a626904
SQUAD_BASE_URL=https://sandbox-api-d.squadco.com
SQUAD_WEBHOOK_SECRET=your_generated_secret_here
SQUAD_PUBLIC_KEY=sandbox_pk_4e0cb820488990016ea2bdd035cf18751b17f9026572
```

### Required for Authentication
```env
JWT_SECRET=fsdbhgevfugeilqgsfcnagvsuyqlXJ1lorkix_ownQGD
JWT_ISSUER=https://localhost:5001
JWT_AUDIENCE=https://localhost:5001
JWT_EXPIRY=1h
```

### Required for Database
```env
ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=squad_db;Username=squad_user;Password=squad_password
```

## 🎯 Next Steps

1. **Generate and configure the webhook secret**
2. **Test the Docker setup**
3. **Verify database connectivity**
4. **Test payment initiation**
5. **Configure webhook URL in Squad.co dashboard**
6. **Test webhook signature validation**

## 📞 Support

If you encounter issues:
1. Check the logs: `docker-compose logs -f squad-api`
2. Verify environment variables
3. Test individual components
4. Check Squad.co documentation for API changes 