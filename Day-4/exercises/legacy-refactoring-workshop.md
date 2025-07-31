# Legacy Code Refactoring Workshop

## 🎯 Workshop Objectives

**Learn industry-standard patterns to break down large codebases (4000+ lines) without breaking existing functionality.**

### 📋 What You'll Learn
- Systematic approach to legacy code analysis
- Domain-Driven Design (DDD) principles
- Refactoring patterns and strategies
- Testing strategies for safe refactoring
- Versioning considerations during refactoring

## 🚀 Workshop Structure

### Phase 1: Legacy Code Analysis (30 minutes)

#### Step 1: Code Assessment
```csharp
// Sample 4000+ line legacy file (simplified for workshop)
public class LegacyPaymentProcessor
{
    // 4000+ lines of mixed responsibilities
    // Payment processing, validation, logging, database operations
    // All in one massive class
}
```

**Analysis Questions**:
1. What are the main responsibilities in this class?
2. What are the dependencies?
3. What are the potential breaking points?
4. How would you test this code?

#### Step 2: Identify Refactoring Opportunities
```markdown
🔍 Code Smells to Look For:
- Long methods (20+ lines)
- Large classes (500+ lines)
- Mixed responsibilities
- Tight coupling
- Duplicate code
- Complex conditional logic
- Hard-coded values
- Poor naming conventions
```

### Phase 2: Refactoring Strategy (45 minutes)

#### Step 1: Extract Method Pattern
```csharp
// Before: Long method with multiple responsibilities
public async Task<PaymentResult> ProcessPayment(PaymentRequest request)
{
    // 50+ lines of mixed logic
    // Validation, processing, logging, database operations
}

// After: Extracted methods
public async Task<PaymentResult> ProcessPayment(PaymentRequest request)
{
    await ValidatePaymentRequest(request);
    var payment = await CreatePayment(request);
    await ProcessPaymentTransaction(payment);
    await LogPaymentActivity(payment);
    return payment.ToResult();
}

private async Task ValidatePaymentRequest(PaymentRequest request) { /* ... */ }
private async Task<Payment> CreatePayment(PaymentRequest request) { /* ... */ }
private async Task ProcessPaymentTransaction(Payment payment) { /* ... */ }
private async Task LogPaymentActivity(Payment payment) { /* ... */ }
```

#### Step 2: Extract Class Pattern
```csharp
// Before: One large class
public class LegacyPaymentProcessor
{
    // 4000+ lines of mixed responsibilities
}

// After: Multiple focused classes
public class PaymentProcessor
{
    private readonly IPaymentValidator _validator;
    private readonly IPaymentRepository _repository;
    private readonly IPaymentLogger _logger;
    private readonly IPaymentGateway _gateway;
    
    public async Task<PaymentResult> ProcessPayment(PaymentRequest request)
    {
        // Orchestration logic only
    }
}

public class PaymentValidator : IPaymentValidator { /* ... */ }
public class PaymentRepository : IPaymentRepository { /* ... */ }
public class PaymentLogger : IPaymentLogger { /* ... */ }
public class PaymentGateway : IPaymentGateway { /* ... */ }
```

#### Step 3: Domain-Driven Design (DDD) Application
```csharp
// Domain Layer
public class Payment : AggregateRoot
{
    public PaymentId Id { get; private set; }
    public Money Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public PaymentMethod Method { get; private set; }
    
    public static Payment Create(PaymentRequest request)
    {
        // Domain logic
    }
    
    public void Process()
    {
        // Domain logic
    }
}

// Application Layer
public class ProcessPaymentCommand : ICommand<PaymentResult>
{
    public PaymentRequest Request { get; set; }
}

public class ProcessPaymentHandler : ICommandHandler<ProcessPaymentCommand, PaymentResult>
{
    private readonly IPaymentRepository _repository;
    private readonly IPaymentGateway _gateway;
    
    public async Task<PaymentResult> Handle(ProcessPaymentCommand command)
    {
        // Application logic
    }
}

// Infrastructure Layer
public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _context;
    
    public async Task<Payment> GetByIdAsync(PaymentId id)
    {
        // Infrastructure logic
    }
}
```

### Phase 3: Testing Strategy (30 minutes)

#### Step 1: Test-First Refactoring
```csharp
[Test]
public async Task ProcessPayment_ValidRequest_ReturnsSuccess()
{
    // Arrange
    var request = new PaymentRequest { /* ... */ };
    var processor = new PaymentProcessor(/* dependencies */);
    
    // Act
    var result = await processor.ProcessPayment(request);
    
    // Assert
    Assert.That(result.IsSuccess, Is.True);
}

[Test]
public async Task ProcessPayment_InvalidRequest_ReturnsFailure()
{
    // Arrange
    var request = new PaymentRequest { /* invalid data */ };
    var processor = new PaymentProcessor(/* dependencies */);
    
    // Act
    var result = await processor.ProcessPayment(request);
    
    // Assert
    Assert.That(result.IsSuccess, Is.False);
    Assert.That(result.Errors, Is.Not.Empty);
}
```

#### Step 2: Integration Testing
```csharp
[Test]
public async Task PaymentFlow_EndToEnd_Success()
{
    // Test the entire payment flow
    // Including external dependencies
}
```

### Phase 4: Versioning Strategy (30 minutes)

#### Step 1: API Versioning During Refactoring
```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class PaymentsControllerV1 : ControllerBase
{
    // Legacy implementation
    [HttpPost]
    public async Task<IActionResult> ProcessPayment(PaymentRequest request)
    {
        // Old implementation
    }
}

[ApiController]
[Route("api/v2/[controller]")]
public class PaymentsControllerV2 : ControllerBase
{
    // New refactored implementation
    [HttpPost]
    public async Task<IActionResult> ProcessPayment(ProcessPaymentCommand command)
    {
        // New implementation
    }
}
```

#### Step 2: Backward Compatibility
```csharp
public class PaymentRequestAdapter
{
    public static ProcessPaymentCommand Adapt(PaymentRequest legacyRequest)
    {
        // Convert legacy request to new command
        return new ProcessPaymentCommand
        {
            Request = legacyRequest
        };
    }
}
```

## 🛠️ Hands-On Exercise

### Exercise 1: Refactor Legacy Code
```csharp
// Given this legacy code, refactor it step by step
public class LegacyUserManager
{
    // 2000+ lines of mixed responsibilities
    // User management, authentication, authorization, logging, database operations
    
    public async Task<UserResult> CreateUser(CreateUserRequest request)
    {
        // 100+ lines of mixed logic
        // Validation, database operations, logging, email sending, etc.
    }
    
    public async Task<UserResult> UpdateUser(UpdateUserRequest request)
    {
        // 150+ lines of mixed logic
    }
    
    public async Task<UserResult> DeleteUser(DeleteUserRequest request)
    {
        // 80+ lines of mixed logic
    }
    
    // ... many more methods
}
```

**Your Task**:
1. Analyze the code and identify responsibilities
2. Apply Extract Method pattern
3. Apply Extract Class pattern
4. Implement DDD principles
5. Add comprehensive tests
6. Implement versioning strategy

### Exercise 2: Squad.API Refactoring
```csharp
// Refactor the Squad.API controllers and services
// Apply the patterns learned in this workshop
```

## 📊 Assessment Criteria

### Refactoring Quality (40%)
- ✅ Clear separation of concerns
- ✅ Single responsibility principle
- ✅ Loose coupling
- ✅ High cohesion
- ✅ Proper abstraction levels

### Testing Coverage (30%)
- ✅ Unit tests for all extracted methods
- ✅ Integration tests for workflows
- ✅ Test coverage > 80%
- ✅ Meaningful test names and assertions

### DDD Implementation (20%)
- ✅ Proper domain modeling
- ✅ Clear bounded contexts
- ✅ Appropriate use of DDD patterns
- ✅ Domain logic in domain layer

### Versioning Strategy (10%)
- ✅ Backward compatibility maintained
- ✅ Clear versioning approach
- ✅ Migration strategy documented
- ✅ Deprecation warnings implemented

## 🎯 Success Metrics

### By End of Workshop, You Should:
1. ✅ Successfully refactor a 4000+ line file
2. ✅ Apply DDD principles correctly
3. ✅ Maintain backward compatibility
4. ✅ Achieve > 80% test coverage
5. ✅ Document refactoring decisions

## 📚 Additional Resources

### Tools & Libraries
- **ReSharper** - Automated refactoring tools
- **SonarQube** - Code quality analysis
- **NDepend** - .NET code analysis
- **xUnit** - Testing framework
- **Moq** - Mocking framework

### Documentation
- [Refactoring: Improving the Design of Existing Code](https://martinfowler.com/books/refactoring.html)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [Clean Code](https://www.amazon.com/Clean-Code-Handbook-Software-Craftsmanship/dp/0132350884)

### Best Practices
- Always write tests before refactoring
- Refactor in small, incremental steps
- Verify functionality after each step
- Document your refactoring decisions
- Consider the impact on other parts of the system

---

**Remember**: Refactoring is an iterative process. Start small, test frequently, and build confidence with each successful refactoring step! 