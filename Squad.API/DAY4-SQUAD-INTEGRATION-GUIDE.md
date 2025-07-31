# Day 4: Squad.co Payment Gateway Integration Guide

## Overview

This guide covers how to integrate Squad.co payment gateway into your API and how to break down large code files effectively.

## 🎯 Learning Objectives

1. **Squad.co Integration**: Understand payment gateway integration patterns
2. **Code Organization**: Learn to break down large files without breaking changes
3. **Security Best Practices**: Implement webhook signature validation
4. **Error Handling**: Proper error handling in payment systems
5. **Response Patterns**: Consistent API response envelopes

## 📋 Prerequisites

- Squad.co account (sandbox for testing)
- API keys from Squad.co dashboard
- Understanding of HTTP webhooks
- Basic knowledge of HMAC signature validation

## 🏗️ Architecture Overview

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend      │    │   Your API      │    │   Squad.co      │
│   Application   │───▶│   (Squad.API)   │───▶│   Payment API   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                              │                        │
                              │                        │
                              ▼                        ▼
                       ┌─────────────────┐    ┌─────────────────┐
                       │   Webhook       │◀───│   Payment       │
                       │   Endpoint      │    │   Notifications │
                       └─────────────────┘    └─────────────────┘
```

## 🔧 Environment Setup

### 1. Environment Variables

Create a `.env` file in your project root:

```env
# Squad.co Configuration
SQUAD_API_KEY=sandbox_sk_your_api_key_here
SQUAD_BASE_URL=https://sandbox-api-d.squadco.com
SQUAD_WEBHOOK_SECRET=your_webhook_secret_here

# JWT Configuration
JWT_SECRET=your_jwt_secret_here
JWT_ISSUER=your_issuer
JWT_AUDIENCE=your_audience

# Other Configuration
BANK_API_KEY=your_bank_api_key
ACCOUNT_LOOKUP=https://api.example.com/lookup
FUND_TRANSFER=https://api.example.com/transfer
REQUERY_TRANSFER=https://api.example.com/requery
GET_ALL_TRANSFERS=https://api.example.com/transfers
```

### 2. Squad.co Dashboard Setup

1. **Get API Keys**: 
   - Log into Squad.co dashboard
   - Navigate to API Keys section
   - Copy your sandbox secret key

2. **Configure Webhooks**:
   - Set webhook URL: `https://your-domain.com/api/squadpayment/webhook`
   - Configure webhook secret
   - Enable events: `charge.success`, `charge.failed`, `transfer.success`

## 🚀 API Endpoints

### 1. Initiate Payment

**Endpoint**: `POST /api/squadpayment/initiate`

**Request Body**:
```json
{
  "email": "customer@example.com",
  "amount": 50000,
  "currency": "NGN",
  "customer_name": "John Doe",
  "transaction_ref": "TXN_123456789",
  "callback_url": "https://your-app.com/payment/callback",
  "payment_channels": ["card", "bank", "ussd"],
  "metadata": {
    "order_id": "ORD_123",
    "customer_id": "CUST_456"
  }
}
```

**Response**:
```json
{
  "success": true,
  "message": "Payment initiated successfully",
  "status": 200,
  "data": {
    "checkout_url": "https://sandbox-pay.squadco.com/TXN_123456789",
    "transaction_ref": "TXN_123456789",
    "transaction_amount": 50000,
    "currency": "NGN",
    "authorized_channels": ["card", "bank", "ussd"]
  }
}
```

### 2. Verify Transaction

**Endpoint**: `GET /api/squadpayment/verify/{transactionRef}`

**Response**:
```json
{
  "success": true,
  "message": "Transaction verified successfully",
  "status": 200,
  "data": {
    "id": 12345,
    "transaction_ref": "TXN_123456789",
    "transaction_amount": 50000,
    "transaction_status": "success",
    "email": "customer@example.com",
    "customer_name": "John Doe",
    "created_at": "2024-01-15T10:30:00Z"
  }
}
```

### 3. Webhook Endpoint

**Endpoint**: `POST /api/squadpayment/webhook`

**Webhook Payload**:
```json
{
  "event": "charge.success",
  "data": {
    "id": 12345,
    "transaction_ref": "TXN_123456789",
    "transaction_amount": 50000,
    "email": "customer@example.com",
    "customer_name": "John Doe",
    "transaction_status": "success",
    "transaction_type": "Card",
    "currency": "NGN",
    "gateway_ref": "GATEWAY_REF_123",
    "created_at": "2024-01-15T10:30:00Z",
    "metadata": {
      "order_id": "ORD_123",
      "customer_id": "CUST_456"
    }
  },
  "signature": "hmac_signature_here"
}
```

## 🔒 Security Implementation

### 1. Webhook Signature Validation

```csharp
public bool ValidateWebhookSignature(string payload, string signature)
{
    using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecret));
    var computedSignature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
    
    return computedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);
}
```

### 2. JWT Authentication

All payment endpoints (except webhooks) require JWT authentication:

```csharp
[Authorize]
public class SquadPaymentController : ControllerBase
{
    // Controller methods
}
```

## 📊 Response Pattern (Envelope Pattern)

We use a consistent response envelope pattern:

```csharp
public class BaseResponse<T>
{
    public bool Success => (int)StatusCode >= 200 && (int)StatusCode < 300;
    public string Message { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public T Data { get; set; }
}
```

**Benefits**:
- Consistent error handling
- Easy client-side parsing
- Clear success/failure indication
- Extensible for additional metadata

## 🧩 Code Organization Principles

### 1. Separation of Concerns

```
Squad.API/
├── Controllers/          # HTTP endpoints
├── Services/            # Business logic
├── Models/              # Data structures
└── Utilities/           # Helper functions
```

### 2. Interface-Based Design

```csharp
public interface ISquadPaymentService
{
    Task<BaseResponse<SquadPaymentResponse>> InitiatePaymentAsync(SquadPaymentRequest request);
    Task<BaseResponse<SquadVerifyResponse>> VerifyTransactionAsync(string transactionRef);
    Task<BaseResponse<object>> ProcessWebhookAsync(SquadWebhookRequest webhookRequest);
}
```

### 3. Dependency Injection

```csharp
services.AddScoped<ISquadPaymentService, SquadPaymentService>();
services.AddHttpClient<ISquadPaymentService, SquadPaymentService>();
```

## 🔄 Breaking Down Large Files

### Strategy 1: Extract Interfaces

**Before** (Large Service):
```csharp
public class PaymentService
{
    // 3000 lines of mixed responsibilities
    public async Task<PaymentResponse> ProcessPayment() { /* ... */ }
    public async Task<RefundResponse> ProcessRefund() { /* ... */ }
    public async Task<WebhookResponse> ProcessWebhook() { /* ... */ }
    // ... many more methods
}
```

**After** (Separated by Responsibility):
```csharp
// Interfaces
public interface IPaymentInitiationService { /* ... */ }
public interface IPaymentVerificationService { /* ... */ }
public interface IWebhookProcessingService { /* ... */ }

// Implementations
public class PaymentInitiationService : IPaymentInitiationService { /* ... */ }
public class PaymentVerificationService : IPaymentVerificationService { /* ... */ }
public class WebhookProcessingService : IWebhookProcessingService { /* ... */ }
```

### Strategy 2: Extract DTOs

**Before**:
```csharp
public class PaymentController
{
    // Inline request/response classes
    public async Task<IActionResult> ProcessPayment([FromBody] object request)
    {
        // Complex inline logic
    }
}
```

**After**:
```csharp
// Separate DTO files
public class SquadPaymentRequest { /* ... */ }
public class SquadPaymentResponse { /* ... */ }
public class SquadVerifyRequest { /* ... */ }
public class SquadVerifyResponse { /* ... */ }

public class SquadPaymentController
{
    public async Task<IActionResult> InitiatePayment([FromBody] SquadPaymentRequest request)
    {
        // Clean, focused logic
    }
}
```

### Strategy 3: Extract Utilities

**Before**:
```csharp
public class PaymentService
{
    // Mixed business logic and utility functions
    private string GenerateSignature(string payload) { /* ... */ }
    private string FormatAmount(decimal amount) { /* ... */ }
    private bool ValidateEmail(string email) { /* ... */ }
    // ... business logic methods
}
```

**After**:
```csharp
// Utility classes
public static class SignatureHelper { /* ... */ }
public static class AmountFormatter { /* ... */ }
public static class ValidationHelper { /* ... */ }

// Clean service
public class PaymentService
{
    // Focused business logic only
}
```

## 🧪 Testing Strategy

### 1. Unit Tests

```csharp
[Test]
public async Task InitiatePayment_ValidRequest_ReturnsSuccess()
{
    // Arrange
    var request = new SquadPaymentRequest { /* ... */ };
    
    // Act
    var result = await _service.InitiatePaymentAsync(request);
    
    // Assert
    Assert.IsTrue(result.Success);
    Assert.IsNotNull(result.Data.CheckoutUrl);
}
```

### 2. Integration Tests

```csharp
[Test]
public async Task Webhook_ValidSignature_ProcessesSuccessfully()
{
    // Arrange
    var webhookPayload = CreateValidWebhookPayload();
    
    // Act
    var result = await _controller.ProcessWebhook(webhookPayload);
    
    // Assert
    Assert.AreEqual(200, result.StatusCode);
}
```

## 🚨 Error Handling

### 1. Payment Failures

```csharp
try
{
    var response = await _squadPaymentService.InitiatePaymentAsync(request);
    return Ok(response);
}
catch (SquadApiException ex)
{
    _logger.LogError(ex, "Squad.co API error");
    return BadRequest(new BaseResponse<object>
    {
        StatusCode = HttpStatusCode.BadRequest,
        Message = "Payment initiation failed"
    });
}
```

### 2. Webhook Validation

```csharp
if (!_squadPaymentService.ValidateWebhookSignature(rawBody, signature))
{
    _logger.LogWarning("Invalid webhook signature");
    return Unauthorized();
}
```

## 📈 Monitoring & Logging

### 1. Structured Logging

```csharp
_logger.LogInformation("Payment initiated for email: {Email}, amount: {Amount}", 
    request.Email, request.Amount);
```

### 2. Performance Monitoring

```csharp
using var activity = _activitySource.StartActivity("SquadPayment.Initiate");
// ... payment logic
```

## 🔄 Migration Strategy

### Phase 1: Add New Endpoints
- Add Squad.co endpoints alongside existing ones
- No breaking changes to existing functionality

### Phase 2: Gradual Migration
- Update clients to use new endpoints
- Monitor usage and performance

### Phase 3: Deprecation
- Mark old endpoints as deprecated
- Provide migration guides

## 📚 Additional Resources

- [Squad.co API Documentation](https://docs.squadco.com/)
- [Payment Gateway Best Practices](https://stripe.com/docs/payments)
- [Webhook Security Guide](https://webhooks.fyi/)
- [API Design Patterns](https://restfulapi.net/)

## 🎯 Key Takeaways

1. **Security First**: Always validate webhook signatures
2. **Consistent Patterns**: Use envelope response pattern
3. **Error Handling**: Comprehensive error handling for payment systems
4. **Code Organization**: Break large files by responsibility
5. **Testing**: Thorough testing for payment functionality
6. **Monitoring**: Proper logging and monitoring for payment systems

---

*This guide provides a foundation for teaching Day 4 with real-world payment gateway integration patterns.* 