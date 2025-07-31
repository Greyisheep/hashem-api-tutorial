# Squad.co Payment API Testing Guide

## Overview
This guide provides comprehensive testing instructions for the Squad.co payment gateway integration, including both Postman collection testing and frontend testing.

## Prerequisites
- Docker and Docker Compose installed
- Python 3.x installed
- Postman installed (for API testing)

## Environment Setup

### 1. Start the Backend Services
```bash
cd Squad.API
docker-compose up -d
```

### 2. Start the Frontend Server
```bash
cd Squad.API
python serve-frontend.py
```

### 3. Verify Services are Running
- API: http://localhost:5000
- Frontend: http://localhost:8081
- PostgreSQL: localhost:5432
- Redis: localhost:6379

## Postman Collection Testing

### Import Collection and Environment
1. Import `Squad.API/postman/Squad-Payment-Collection.json`
2. Import `Squad.API/postman/Squad-Payment-Environment.json`
3. Select the "Squad.co Payment API - Environment" environment

### Testing Sequence

#### 1. Authentication (Optional)
- **Request**: `POST /api/auth/log_in`
- **Body**: 
```json
{
  "email": "admin@example.com",
  "password": "your_password_here"
}
```
- **Note**: Most endpoints are now `[AllowAnonymous]` for testing

#### 2. Payment Modal Testing
- **Request**: `POST /api/squadpayment/initiate`
- **Body**:
```json
{
  "email": "customer@example.com",
  "amount": 50000,
  "currency": "NGN",
  "customer_name": "John Doe",
  "transaction_ref": "TXN_{{timestamp}}",
  "callback_url": "http://localhost:5000/api/squadpayment/redirect",
  "payment_channels": ["card", "bank", "ussd"],
  "metadata": {
    "order_id": "ORD_123",
    "customer_id": "CUST_456"
  },
  "pass_charge": false,
  "is_recurring": false
}
```

#### 3. Direct Payment Testing

##### Card Payment
- **Request**: `POST /api/squadpayment/direct/card`
- **Body**:
```json
{
  "amount": 50000,
  "currency": "NGN",
  "pass_charge": false,
  "webhook_url": "http://localhost:5000/api/squadpayment/webhook",
  "card": {
    "number": "4242424242424242",
    "cvv": "123",
    "expiry_month": "12",
    "expiry_year": "25"
  },
  "payment_method": "card",
  "customer": {
    "name": "John Doe",
    "email": "customer@example.com"
  },
  "redirect_url": "http://localhost:5000/api/squadpayment/redirect"
}
```

##### Bank Payment
- **Request**: `POST /api/squadpayment/direct/bank`
- **Body**:
```json
{
  "amount": 50000,
  "currency": "NGN",
  "pass_charge": false,
  "webhook_url": "http://localhost:5000/api/squadpayment/webhook",
  "bank": {
    "bank_code": "058",
    "account_or_phoneno": "1234567890"
  },
  "payment_method": "bank",
  "customer": {
    "name": "John Doe",
    "email": "customer@example.com"
  }
}
```

##### USSD Payment
- **Request**: `POST /api/squadpayment/direct/ussd`
- **Body**:
```json
{
  "amount": 50000,
  "currency": "NGN",
  "pass_charge": false,
  "webhook_url": "http://localhost:5000/api/squadpayment/webhook",
  "ussd": {
    "bank_code": "058"
  },
  "payment_method": "ussd",
  "customer": {
    "name": "John Doe",
    "email": "customer@example.com"
  }
}
```

#### 4. Transaction Verification
- **Request**: `GET /api/squadpayment/verify/{transaction_ref}`
- **Note**: Use the transaction reference from previous payment requests

#### 5. Webhook Testing
- **Request**: `POST /api/squadpayment/webhook`
- **Headers**: `x-squad-encrypted-body: {{webhook_signature}}`
- **Body**: See collection for sample webhook payloads

## Frontend Testing

### 1. Access the Frontend
Open http://localhost:8081 in your browser

### 2. Test Payment Modal
1. Click "Initiate Payment Modal"
2. Fill in payment details
3. Complete payment on Squad.co modal
4. Verify redirect back to frontend
5. Check transaction verification

### 3. Test Direct Payments
1. Choose payment method (Card/Bank/USSD)
2. Fill in payment details
3. Submit payment
4. Verify redirect and transaction status

### 4. Test Transaction Verification
1. After any payment, click "Verify Payment"
2. Check transaction details
3. Verify email notification (check logs)

## Environment Variables Verification

### Required Variables
```bash
# Squad.co Configuration
SQUAD_API_KEY=sandbox_sk_4e0cb820488990016ea2bdd648ca077a13059a626904
SQUAD_BASE_URL=https://sandbox-api-d.squadco.com
SQUAD_WEBHOOK_SECRET=your_webhook_secret_here
SQUAD_PUBLIC_KEY=sandbox_pk_4e0cb820488990016ea2bdd035cf18751b17f9026572

# Notification Configuration
NOTIFICATION_EMAIL=greyisheep@gmail.com

# JWT Configuration
JWT_SECRET=fsdbhgevfugeilqgsfcnagvsuyqlXJ1lorkix_ownQGD
JWT_ISSUER=https://localhost:5001
JWT_AUDIENCE=https://localhost:5001
JWT_EXPIRY=1h

# Database Configuration
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=squad_db;Username=squad_user;Password=squad_password

# Application Configuration
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:7001;http://localhost:5001
```

## Testing Checklist

### API Endpoints
- [ ] `POST /api/squadpayment/initiate` - Payment modal initiation
- [ ] `GET /api/squadpayment/verify/{transactionRef}` - Transaction verification
- [ ] `GET /api/squadpayment/redirect` - Payment redirect handling
- [ ] `POST /api/squadpayment/webhook` - Webhook processing
- [ ] `POST /api/squadpayment/direct/card` - Direct card payment
- [ ] `POST /api/squadpayment/direct/bank` - Direct bank payment
- [ ] `POST /api/squadpayment/direct/ussd` - Direct USSD payment

### Frontend Features
- [ ] Payment modal integration
- [ ] Direct payment forms
- [ ] Redirect handling
- [ ] Transaction verification
- [ ] Error handling
- [ ] Success/failure messages

### Security Features
- [ ] Webhook signature validation
- [ ] HMAC-SHA512 verification
- [ ] JWT authentication (where applicable)
- [ ] Input validation

### Integration Features
- [ ] Email notifications
- [ ] Database logging
- [ ] Error logging
- [ ] Transaction status tracking

## Troubleshooting

### Common Issues
1. **API not responding**: Check Docker containers with `docker ps`
2. **Frontend not loading**: Verify Python server is running on port 8081
3. **Authentication errors**: Most endpoints are now `[AllowAnonymous]` for testing
4. **Webhook failures**: Check signature validation and payload format
5. **Database connection**: Verify PostgreSQL container is healthy

### Logs
- API logs: `docker logs squad-api`
- Database logs: `docker logs squad-postgres`
- Redis logs: `docker logs squad-redis`

## Success Criteria
- All API endpoints respond correctly
- Frontend can initiate and complete payments
- Redirects work properly
- Webhooks are processed and validated
- Email notifications are sent (check logs)
- Transaction verification returns correct data
- Error handling works for invalid requests

## Next Steps
1. Test with real Squad.co sandbox credentials
2. Implement actual email sending logic
3. Add comprehensive error handling
4. Implement rate limiting
5. Add monitoring and alerting 