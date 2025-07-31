# Day 4: Squad.co Payment Gateway Integration

## 🎯 Teaching Overview

This implementation demonstrates real-world payment gateway integration with Squad.co, covering:

1. **Payment Gateway Integration**: Complete Squad.co API integration
2. **Code Organization**: Breaking down large files effectively
3. **Security Best Practices**: Webhook signature validation
4. **Response Patterns**: Consistent API envelope patterns
5. **Error Handling**: Comprehensive error handling for payment systems

## 🏗️ Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Frontend      │    │   Squad.API     │    │   Squad.co      │
│   Application   │───▶│   (Your API)    │───▶│   Payment API   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                              │                        │
                              │                        │
                              ▼                        ▼
                       ┌─────────────────┐    ┌─────────────────┐
                       │   Webhook       │◀───│   Payment       │
                       │   Endpoint      │    │   Notifications │
                       └─────────────────┘    └─────────────────┘
```

## 🚀 Quick Start

### 1. Environment Setup

Copy the example environment file:
```bash
cp env.example .env
```

Update `.env` with your Squad.co credentials:
```env
SQUAD_API_KEY=sandbox_sk_your_actual_api_key
SQUAD_WEBHOOK_SECRET=your_webhook_secret
JWT_SECRET=your_secure_jwt_secret
```

### 2. Run the Application

```bash
cd Squad.API
dotnet run
```

The API will be available at:
- HTTPS: https://localhost:7001
- HTTP: http://localhost:5001
- Swagger: https://localhost:7001/swagger

### 3. Test with Postman

Import the Postman collection:
- File: `postman/Squad-Payment-Collection.json`
- Environment variables will be auto-populated

## 📚 Teaching Points

### 1. Payment Gateway Integration

**Key Concepts**:
- **Payment Initiation**: Creating payment requests
- **Transaction Verification**: Checking payment status
- **Webhook Processing**: Handling payment notifications
- **Security**: Signature validation for webhooks

**Code Example**:
```csharp
// Payment initiation
public async Task<BaseResponse<SquadPaymentResponse>> InitiatePaymentAsync(SquadPaymentRequest request)
{
    // Generate transaction reference
    if (string.IsNullOrEmpty(request.TransactionRef))
    {
        request.TransactionRef = GenerateTransactionReference();
    }

    // Call Squad.co API
    var response = await _httpClient.PostAsync($"{_squadBaseUrl}/transaction/initiate", content);
    
    // Process response
    if (response.IsSuccessStatusCode)
    {
        var squadResponse = JsonSerializer.Deserialize<SquadPaymentResponse>(responseContent);
        return new BaseResponse<SquadPaymentResponse>
        {
            StatusCode = HttpStatusCode.OK,
            Message = "Payment initiated successfully",
            Data = squadResponse
        };
    }
}
```

### 2. Code Organization Principles

**Strategy 1: Extract Interfaces**
```csharp
// Before: Large service with mixed responsibilities
public class PaymentService
{
    // 3000 lines of mixed methods
}

// After: Separated by responsibility
public interface IPaymentInitiationService { }
public interface IPaymentVerificationService { }
public interface IWebhookProcessingService { }
```

**Strategy 2: Extract DTOs**
```csharp
// Separate files for each DTO
public class SquadPaymentRequest { }
public class SquadPaymentResponse { }
public class SquadVerifyRequest { }
public class SquadVerifyResponse { }
```

**Strategy 3: Extract Utilities**
```csharp
// Utility classes for common functions
public static class SignatureHelper { }
public static class AmountFormatter { }
public static class ValidationHelper { }
```

### 3. Security Implementation

**Webhook Signature Validation**:
```csharp
public bool ValidateWebhookSignature(string payload, string signature)
{
    using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecret));
    var computedSignature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
    
    return computedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);
}
```

**JWT Authentication**:
```csharp
[Authorize]
public class SquadPaymentController : ControllerBase
{
    // All endpoints require authentication except webhooks
}
```

### 4. Response Pattern (Envelope Pattern)

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

## 🔄 Breaking Down Large Files

### Step-by-Step Process

1. **Identify Responsibilities**
   - Group methods by functionality
   - Identify common patterns
   - Separate concerns

2. **Extract Interfaces**
   - Create interfaces for each responsibility
   - Define clear contracts
   - Enable dependency injection

3. **Create Separate Files**
   - One file per interface/implementation
   - Clear naming conventions
   - Proper namespacing

4. **Update Dependencies**
   - Register new services in DI container
   - Update controller dependencies
   - Maintain backward compatibility

5. **Test Incrementally**
   - Test each extracted component
   - Ensure no breaking changes
   - Validate functionality

### Example: Breaking Down a Large Service

**Before** (3000 lines):
```csharp
public class PaymentService
{
    // Payment initiation methods
    public async Task<PaymentResponse> InitiatePayment() { /* 500 lines */ }
    public async Task<PaymentResponse> InitiateRecurringPayment() { /* 300 lines */ }
    
    // Verification methods
    public async Task<VerificationResponse> VerifyPayment() { /* 400 lines */ }
    public async Task<VerificationResponse> VerifyRefund() { /* 200 lines */ }
    
    // Webhook methods
    public async Task<WebhookResponse> ProcessWebhook() { /* 600 lines */ }
    public async Task<WebhookResponse> ProcessRefundWebhook() { /* 400 lines */ }
    
    // Utility methods
    private string GenerateSignature() { /* 100 lines */ }
    private string FormatAmount() { /* 50 lines */ }
    private bool ValidateEmail() { /* 50 lines */ }
    
    // ... 300 more lines
}
```

**After** (Separated by Responsibility):
```csharp
// Interfaces
public interface IPaymentInitiationService
{
    Task<PaymentResponse> InitiatePaymentAsync(PaymentRequest request);
    Task<PaymentResponse> InitiateRecurringPaymentAsync(RecurringPaymentRequest request);
}

public interface IPaymentVerificationService
{
    Task<VerificationResponse> VerifyPaymentAsync(string transactionRef);
    Task<VerificationResponse> VerifyRefundAsync(string refundRef);
}

public interface IWebhookProcessingService
{
    Task<WebhookResponse> ProcessPaymentWebhookAsync(WebhookRequest request);
    Task<WebhookResponse> ProcessRefundWebhookAsync(WebhookRequest request);
}

// Implementations
public class PaymentInitiationService : IPaymentInitiationService { /* 800 lines */ }
public class PaymentVerificationService : IPaymentVerificationService { /* 600 lines */ }
public class WebhookProcessingService : IWebhookProcessingService { /* 1000 lines */ }

// Utilities
public static class SignatureHelper { /* 100 lines */ }
public static class AmountFormatter { /* 50 lines */ }
public static class ValidationHelper { /* 50 lines */ }
```

## 🧪 Testing Strategy

### 1. Unit Tests

```csharp
[Test]
public async Task InitiatePayment_ValidRequest_ReturnsSuccess()
{
    // Arrange
    var request = new SquadPaymentRequest 
    { 
        Email = "test@example.com",
        Amount = 50000,
        Currency = "NGN"
    };
    
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

## 📊 API Endpoints

### Payment Operations

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/squadpayment/initiate` | Initiate payment | Yes |
| GET | `/api/squadpayment/verify/{ref}` | Verify transaction | Yes |
| POST | `/api/squadpayment/webhook` | Webhook endpoint | No |

### Legacy Operations

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/habari/lookup` | Account lookup | Yes |
| POST | `/api/habari/transfer` | Transfer funds | Yes |
| POST | `/api/habari/requery` | Requery transaction | Yes |
| GET | `/api/habari/get_all_transfers` | Get all transfers | Yes |

## 🔒 Security Considerations

1. **Webhook Security**
   - Always validate webhook signatures
   - Use HTTPS for webhook endpoints
   - Implement idempotency

2. **API Security**
   - JWT authentication for all endpoints
   - Rate limiting for payment endpoints
   - Input validation and sanitization

3. **Data Security**
   - Encrypt sensitive data
   - Log security events
   - Monitor for suspicious activity

## 📈 Monitoring & Logging

### Structured Logging
```csharp
_logger.LogInformation("Payment initiated for email: {Email}, amount: {Amount}", 
    request.Email, request.Amount);
```

### Performance Monitoring
```csharp
using var activity = _activitySource.StartActivity("SquadPayment.Initiate");
// ... payment logic
```

## 🚨 Error Handling

### Payment Failures
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

### Webhook Validation
```csharp
if (!_squadPaymentService.ValidateWebhookSignature(rawBody, signature))
{
    _logger.LogWarning("Invalid webhook signature");
    return Unauthorized();
}
```

## 🔄 Migration Strategy

### Phase 1: Add New Endpoints
- Add Squad.co endpoints alongside existing ones
- No breaking changes to existing functionality
- Maintain backward compatibility

### Phase 2: Gradual Migration
- Update clients to use new endpoints
- Monitor usage and performance
- Collect feedback and iterate

### Phase 3: Deprecation
- Mark old endpoints as deprecated
- Provide migration guides
- Plan for eventual removal

## 📚 Additional Resources

- [Squad.co API Documentation](https://docs.squadco.com/)
- [Payment Gateway Best Practices](https://stripe.com/docs/payments)
- [Webhook Security Guide](https://webhooks.fyi/)
- [API Design Patterns](https://restfulapi.net/)

## 🎯 Key Takeaways for Students

1. **Security First**: Always validate webhook signatures and implement proper authentication
2. **Consistent Patterns**: Use envelope response patterns for consistent API design
3. **Error Handling**: Comprehensive error handling is crucial for payment systems
4. **Code Organization**: Break large files by responsibility, not just size
5. **Testing**: Thorough testing is essential for payment functionality
6. **Monitoring**: Proper logging and monitoring for payment systems
7. **Documentation**: Clear documentation helps with maintenance and onboarding

---

*This implementation provides a solid foundation for teaching real-world payment gateway integration and code organization principles.* 