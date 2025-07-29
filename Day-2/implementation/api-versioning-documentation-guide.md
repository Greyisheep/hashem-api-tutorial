# API Versioning & Documentation Implementation Guide - .NET

## 🎯 Implementation Objectives
- Implement multiple API versioning strategies in .NET
- Generate comprehensive OpenAPI documentation
- Set up API contract testing
- Create production-ready API documentation

## ⏰ Implementation Timing: 45 minutes

---

## 🔄 API Versioning Strategies

### Step 1: Install Versioning Packages

```bash
dotnet add package Microsoft.AspNetCore.Mvc.Versioning
dotnet add package Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer
dotnet add package Swashbuckle.AspNetCore
dotnet add package Swashbuckle.AspNetCore.Annotations
```

### Step 2: URL Path Versioning

#### Program.cs Configuration
```csharp
// Program.cs
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

var builder = WebApplication.CreateBuilder(args);

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-API-Version"),
        new MediaTypeApiVersionReader("version")
    );
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Add Swagger with versioning
builder.Services.AddSwaggerGen(options =>
{
    var provider = builder.Services.BuildServiceProvider()
        .GetRequiredService<IApiVersionDescriptionProvider>();

    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerDoc(
            description.GroupName,
            new OpenApiInfo
            {
                Title = $"TaskFlow API {description.ApiVersion}",
                Version = description.ApiVersion.ToString(),
                Description = description.IsDeprecated 
                    ? $"TaskFlow API {description.ApiVersion} - DEPRECATED"
                    : $"TaskFlow API {description.ApiVersion}",
                Contact = new OpenApiContact
                {
                    Name = "TaskFlow Team",
                    Email = "api@taskflow.com",
                    Url = new Uri("https://taskflow.com/api")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });
    }

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // Add operation filters
    options.OperationFilter<SwaggerDefaultValues>();
    options.OperationFilter<AddRequiredHeaderParameter>();
});

var app = builder.Build();

// Configure Swagger UI
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint(
            $"/swagger/{description.GroupName}/swagger.json",
            description.GroupName.ToUpperInvariant());
    }

    options.RoutePrefix = "api-docs";
    options.DocumentTitle = "TaskFlow API Documentation";
    options.DefaultModelsExpandDepth(2);
    options.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
});
```

### Step 3: Versioned Controllers

#### V1 Tasks Controller
```csharp
// TaskFlow.API/Controllers/V1/TasksController.cs
namespace TaskFlow.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TasksController> _logger;

    public TasksController(IMediator mediator, ILogger<TasksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new task
    /// </summary>
    /// <param name="command">The task creation command</param>
    /// <returns>The created task</returns>
    /// <response code="201">Task created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="422">Validation errors</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateTaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [SwaggerOperation(
        Summary = "Create a new task",
        Description = "Creates a new task with the specified details",
        OperationId = "CreateTask",
        Tags = new[] { "Tasks" }
    )]
    public async Task<ActionResult<CreateTaskResponse>> CreateTask(CreateTaskCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTask), new { id = result.TaskId, version = "1.0" }, result);
    }

    /// <summary>
    /// Retrieves a task by ID
    /// </summary>
    /// <param name="id">The task ID</param>
    /// <returns>The task details</returns>
    /// <response code="200">Task found</response>
    /// <response code="404">Task not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetTaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Get task by ID",
        Description = "Retrieves a task by its unique identifier",
        OperationId = "GetTask",
        Tags = new[] { "Tasks" }
    )]
    public async Task<ActionResult<GetTaskResponse>> GetTask(string id)
    {
        var query = new GetTaskQuery { TaskId = id };
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Updates a task
    /// </summary>
    /// <param name="id">The task ID</param>
    /// <param name="command">The update command</param>
    /// <returns>No content</returns>
    /// <response code="204">Task updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Task not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Update a task",
        Description = "Updates an existing task with new information",
        OperationId = "UpdateTask",
        Tags = new[] { "Tasks" }
    )]
    public async Task<ActionResult> UpdateTask(string id, UpdateTaskCommand command)
    {
        command.TaskId = id;
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Deletes a task
    /// </summary>
    /// <param name="id">The task ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Task deleted successfully</response>
    /// <response code="404">Task not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Delete a task",
        Description = "Permanently deletes a task",
        OperationId = "DeleteTask",
        Tags = new[] { "Tasks" }
    )]
    public async Task<ActionResult> DeleteTask(string id)
    {
        var command = new DeleteTaskCommand { TaskId = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
```

#### V2 Tasks Controller (Enhanced)
```csharp
// TaskFlow.API/Controllers/V2/TasksController.cs
namespace TaskFlow.API.Controllers.V2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TasksController> _logger;

    public TasksController(IMediator mediator, ILogger<TasksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new task with enhanced features
    /// </summary>
    /// <param name="command">The enhanced task creation command</param>
    /// <returns>The created task with additional metadata</returns>
    /// <response code="201">Task created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="422">Validation errors</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateTaskV2Response), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [SwaggerOperation(
        Summary = "Create a new task (v2)",
        Description = "Creates a new task with enhanced features including tags, priority, and estimated duration",
        OperationId = "CreateTaskV2",
        Tags = new[] { "Tasks" }
    )]
    public async Task<ActionResult<CreateTaskV2Response>> CreateTask(CreateTaskV2Command command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTask), new { id = result.TaskId, version = "2.0" }, result);
    }

    /// <summary>
    /// Retrieves a task by ID with enhanced details
    /// </summary>
    /// <param name="id">The task ID</param>
    /// <param name="includeComments">Include task comments</param>
    /// <param name="includeHistory">Include task history</param>
    /// <returns>The enhanced task details</returns>
    /// <response code="200">Task found</response>
    /// <response code="404">Task not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GetTaskV2Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Get task by ID (v2)",
        Description = "Retrieves a task with enhanced details including comments, history, and metadata",
        OperationId = "GetTaskV2",
        Tags = new[] { "Tasks" }
    )]
    public async Task<ActionResult<GetTaskV2Response>> GetTask(
        string id, 
        [FromQuery] bool includeComments = false,
        [FromQuery] bool includeHistory = false)
    {
        var query = new GetTaskV2Query 
        { 
            TaskId = id,
            IncludeComments = includeComments,
            IncludeHistory = includeHistory
        };
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Assigns a task to a user
    /// </summary>
    /// <param name="id">The task ID</param>
    /// <param name="command">The assignment command</param>
    /// <returns>No content</returns>
    /// <response code="204">Task assigned successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Task not found</response>
    /// <response code="409">Task already assigned</response>
    [HttpPost("{id}/assign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [SwaggerOperation(
        Summary = "Assign task to user",
        Description = "Assigns a task to a specific user",
        OperationId = "AssignTask",
        Tags = new[] { "Tasks" }
    )]
    public async Task<ActionResult> AssignTask(string id, AssignTaskCommand command)
    {
        command.TaskId = id;
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Adds a comment to a task
    /// </summary>
    /// <param name="id">The task ID</param>
    /// <param name="command">The comment command</param>
    /// <returns>The created comment</returns>
    /// <response code="201">Comment added successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Task not found</response>
    [HttpPost("{id}/comments")]
    [ProducesResponseType(typeof(CommentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Add comment to task",
        Description = "Adds a new comment to an existing task",
        OperationId = "AddComment",
        Tags = new[] { "Tasks" }
    )]
    public async Task<ActionResult<CommentResponse>> AddComment(string id, AddCommentCommand command)
    {
        command.TaskId = id;
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTask), new { id, version = "2.0" }, result);
    }
}
```

### Step 4: Header Versioning Example

#### Header Versioning Controller
```csharp
// TaskFlow.API/Controllers/HeaderVersionedController.cs
namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class HeaderVersionedController : ControllerBase
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    public ActionResult<string> GetV1()
    {
        return Ok("API Version 1.0 - Use X-API-Version header");
    }

    [HttpGet]
    [MapToApiVersion("2.0")]
    public ActionResult<string> GetV2()
    {
        return Ok("API Version 2.0 - Use X-API-Version header");
    }
}
```

---

## 📚 OpenAPI Documentation Enhancement

### Step 5: Swagger Operation Filters

#### Swagger Default Values Filter
```csharp
// TaskFlow.API/Filters/SwaggerDefaultValues.cs
namespace TaskFlow.API.Filters;

public class SwaggerDefaultValues : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;

        operation.Deprecated |= apiDescription.IsDeprecated();

        // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1752#issue-663991077
        foreach (var responseType in context.ApiDescription.SupportedResponseTypes)
        {
            var responseKey = responseType.IsDefaultResponse ? "default" : responseType.StatusCode.ToString();
            var response = operation.Responses[responseKey];

            foreach (var contentType in response.Content.Keys)
            {
                if (!responseType.ApiResponseFormats.Any(x => x.MediaType == contentType))
                {
                    response.Content.Remove(contentType);
                }
            }
        }
    }
}
```

#### Add Required Header Parameter Filter
```csharp
// TaskFlow.API/Filters/AddRequiredHeaderParameter.cs
namespace TaskFlow.API.Filters;

public class AddRequiredHeaderParameter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
            operation.Parameters = new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Correlation-ID",
            In = ParameterLocation.Header,
            Description = "Correlation ID for request tracking",
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Format = "uuid"
            }
        });

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-API-Version",
            In = ParameterLocation.Header,
            Description = "API version (optional, defaults to URL version)",
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Example = new OpenApiString("2.0")
            }
        });
    }
}
```

### Step 6: Enhanced Response Models

#### V1 Response Models
```csharp
// TaskFlow.Application/DTOs/V1/CreateTaskResponse.cs
namespace TaskFlow.Application.DTOs.V1;

/// <summary>
/// Response model for task creation (v1)
/// </summary>
public record CreateTaskResponse
{
    /// <summary>
    /// The unique identifier of the created task
    /// </summary>
    /// <example>task_001</example>
    public string TaskId { get; init; } = string.Empty;

    /// <summary>
    /// The title of the task
    /// </summary>
    /// <example>Implement user authentication</example>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// The current status of the task
    /// </summary>
    /// <example>pending</example>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// The timestamp when the task was created
    /// </summary>
    /// <example>2024-01-15T10:30:00Z</example>
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Response model for task retrieval (v1)
/// </summary>
public record GetTaskResponse
{
    /// <summary>
    /// The unique identifier of the task
    /// </summary>
    /// <example>task_001</example>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The title of the task
    /// </summary>
    /// <example>Implement user authentication</example>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// The description of the task
    /// </summary>
    /// <example>Implement JWT-based authentication for the API</example>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// The current status of the task
    /// </summary>
    /// <example>in_progress</example>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// The ID of the user assigned to the task
    /// </summary>
    /// <example>user_123</example>
    public string? AssigneeId { get; init; }

    /// <summary>
    /// The ID of the project the task belongs to
    /// </summary>
    /// <example>project_456</example>
    public string ProjectId { get; init; } = string.Empty;

    /// <summary>
    /// The timestamp when the task was created
    /// </summary>
    /// <example>2024-01-15T10:30:00Z</example>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The timestamp when the task was last updated
    /// </summary>
    /// <example>2024-01-16T14:45:00Z</example>
    public DateTime UpdatedAt { get; init; }
}
```

#### V2 Response Models (Enhanced)
```csharp
// TaskFlow.Application/DTOs/V2/CreateTaskV2Response.cs
namespace TaskFlow.Application.DTOs.V2;

/// <summary>
/// Enhanced response model for task creation (v2)
/// </summary>
public record CreateTaskV2Response
{
    /// <summary>
    /// The unique identifier of the created task
    /// </summary>
    /// <example>task_001</example>
    public string TaskId { get; init; } = string.Empty;

    /// <summary>
    /// The title of the task
    /// </summary>
    /// <example>Implement user authentication</example>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// The current status of the task
    /// </summary>
    /// <example>pending</example>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// The priority level of the task
    /// </summary>
    /// <example>high</example>
    public string Priority { get; init; } = string.Empty;

    /// <summary>
    /// The estimated duration in hours
    /// </summary>
    /// <example>8</example>
    public int? EstimatedHours { get; init; }

    /// <summary>
    /// The tags associated with the task
    /// </summary>
    /// <example>["authentication", "security", "api"]</example>
    public List<string> Tags { get; init; } = new();

    /// <summary>
    /// The timestamp when the task was created
    /// </summary>
    /// <example>2024-01-15T10:30:00Z</example>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Additional metadata about the task
    /// </summary>
    public TaskMetadata Metadata { get; init; } = new();
}

/// <summary>
/// Task metadata information
/// </summary>
public record TaskMetadata
{
    /// <summary>
    /// The user who created the task
    /// </summary>
    /// <example>user_123</example>
    public string CreatedBy { get; init; } = string.Empty;

    /// <summary>
    /// The project the task belongs to
    /// </summary>
    /// <example>project_456</example>
    public string ProjectId { get; init; } = string.Empty;

    /// <summary>
    /// The sprint the task is assigned to
    /// </summary>
    /// <example>sprint_2024_01</example>
    public string? SprintId { get; init; }

    /// <summary>
    /// The epic the task belongs to
    /// </summary>
    /// <example>epic_auth</example>
    public string? EpicId { get; init; }
}
```

---

## 🔍 API Contract Testing

### Step 7: Contract Testing Setup

#### Install Contract Testing Packages
```bash
dotnet add package PactNet
dotnet add package PactNet.Output.Xunit
```

#### Consumer Contract Tests
```csharp
// TaskFlow.Tests/Contracts/TaskApiConsumerTests.cs
namespace TaskFlow.Tests.Contracts;

public class TaskApiConsumerTests : IClassFixture<ConsumerPactBuilderFixture>
{
    private readonly ConsumerPactBuilderFixture _fixture;

    public TaskApiConsumerTests(ConsumerPactBuilderFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateTask_WithValidData_ShouldReturnCreatedTask()
    {
        // Arrange
        var pact = _fixture.PactBuilder
            .ServiceConsumer("TaskFlow-WebApp")
            .HasPactWith("TaskFlow-API");

        pact.UponReceiving("A request to create a task")
            .Given("A valid task creation request")
            .WithRequest(HttpMethod.Post, "/api/v1/tasks")
            .WithHeader("Content-Type", "application/json")
            .WithJsonBody(new
            {
                title = "Test Task",
                description = "Test Description",
                projectId = "project_123"
            })
            .WillRespond()
            .WithStatus(HttpStatusCode.Created)
            .WithHeader("Content-Type", "application/json")
            .WithJsonBody(new
            {
                taskId = "task_001",
                title = "Test Task",
                status = "pending",
                createdAt = "2024-01-15T10:30:00Z"
            });

        await pact.VerifyAsync(async ctx =>
        {
            // Act
            var client = new HttpClient { BaseAddress = ctx.MockServerUri };
            var response = await client.PostAsJsonAsync("/api/v1/tasks", new
            {
                title = "Test Task",
                description = "Test Description",
                projectId = "project_123"
            });

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("task_001");
        });
    }

    [Fact]
    public async Task GetTask_WithValidId_ShouldReturnTask()
    {
        // Arrange
        var pact = _fixture.PactBuilder
            .ServiceConsumer("TaskFlow-WebApp")
            .HasPactWith("TaskFlow-API");

        pact.UponReceiving("A request to get a task")
            .Given("A task exists with the given ID")
            .WithRequest(HttpMethod.Get, "/api/v1/tasks/task_001")
            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithHeader("Content-Type", "application/json")
            .WithJsonBody(new
            {
                id = "task_001",
                title = "Test Task",
                description = "Test Description",
                status = "in_progress",
                assigneeId = "user_123",
                projectId = "project_123",
                createdAt = "2024-01-15T10:30:00Z",
                updatedAt = "2024-01-16T14:45:00Z"
            });

        await pact.VerifyAsync(async ctx =>
        {
            // Act
            var client = new HttpClient { BaseAddress = ctx.MockServerUri };
            var response = await client.GetAsync("/api/v1/tasks/task_001");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("task_001");
        });
    }
}
```

#### Consumer Pact Builder Fixture
```csharp
// TaskFlow.Tests/Contracts/ConsumerPactBuilderFixture.cs
namespace TaskFlow.Tests.Contracts;

public class ConsumerPactBuilderFixture : IDisposable
{
    public IPactBuilder PactBuilder { get; }

    public ConsumerPactBuilderFixture()
    {
        var config = new PactConfig
        {
            PactDir = Path.Join("..", "..", "..", "pacts"),
            DefaultJsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }
        };

        PactBuilder = Pact.V3("TaskFlow-WebApp", "TaskFlow-API", config);
    }

    public void Dispose()
    {
        PactBuilder?.Dispose();
    }
}
```

### Step 8: Provider Contract Tests

#### Provider Contract Verification
```csharp
// TaskFlow.Tests/Contracts/TaskApiProviderTests.cs
namespace TaskFlow.Tests.Contracts;

public class TaskApiProviderTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TaskApiProviderTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task VerifyPacts()
    {
        // Arrange
        var client = _factory.CreateClient();
        var verifier = new PactVerifier(new PactVerifierConfig());

        // Act & Assert
        verifier
            .ServiceProvider("TaskFlow-API", client)
            .HonoursPactWith("TaskFlow-WebApp")
            .PactUri(Path.Join("..", "..", "..", "pacts", "TaskFlow-WebApp-TaskFlow-API.json"))
            .Verify();
    }
}
```

---

## 📋 Configuration Files

### Step 9: Enhanced appsettings.json
```json
{
  "ApiVersioning": {
    "DefaultVersion": "1.0",
    "AssumeDefaultVersionWhenUnspecified": true,
    "ReportApiVersions": true,
    "SupportedVersions": ["1.0", "2.0"],
    "DeprecatedVersions": []
  },
  "Swagger": {
    "Title": "TaskFlow API",
    "Description": "A comprehensive task management API",
    "Version": "1.0.0",
    "Contact": {
      "Name": "TaskFlow Team",
      "Email": "api@taskflow.com",
      "Url": "https://taskflow.com/api"
    },
    "License": {
      "Name": "MIT",
      "Url": "https://opensource.org/licenses/MIT"
    },
    "Security": {
      "Bearer": {
        "Type": "http",
        "Scheme": "bearer",
        "BearerFormat": "JWT"
      }
    }
  },
  "ContractTesting": {
    "PactBroker": {
      "Url": "http://localhost:9292",
      "PublishResults": true
    },
    "Consumer": {
      "Name": "TaskFlow-WebApp",
      "Version": "1.0.0"
    },
    "Provider": {
      "Name": "TaskFlow-API",
      "Version": "1.0.0"
    }
  }
}
```

### Step 10: Project File Configuration
```xml
<!-- TaskFlow.API.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Versioning" Version="5.1.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer" Version="5.1.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    <PackageReference Include="Swashbuckle.AspNetCore.Annotations" Version="6.5.0" />
  </ItemGroup>
</Project>
```

---

## 🎯 Key Learning Points

### API Versioning Strategies
1. **URL Path Versioning**: `/api/v1/tasks` - Most common, clear versioning
2. **Header Versioning**: `X-API-Version: 2.0` - Clean URLs, version in headers
3. **Query Parameter Versioning**: `?version=2.0` - Simple but less clean
4. **Content Negotiation**: `Accept: application/vnd.api+json;version=2.0` - RESTful but complex

### Versioning Best Practices
1. **Plan for Evolution**: Design APIs with versioning in mind
2. **Backward Compatibility**: Maintain compatibility when possible
3. **Deprecation Strategy**: Clear deprecation timeline
4. **Documentation**: Comprehensive version documentation

### OpenAPI Documentation Benefits
1. **Interactive Testing**: Swagger UI for API exploration
2. **Code Generation**: Client SDK generation
3. **Contract Validation**: Ensure API compliance
4. **Developer Experience**: Self-documenting APIs

### Contract Testing Benefits
1. **Consumer-Driven**: Frontend teams define expected behavior
2. **Integration Safety**: Catch breaking changes early
3. **Documentation**: Living documentation of API contracts
4. **CI/CD Integration**: Automated contract validation

---

**Implementation Success**: Students can implement comprehensive API versioning strategies, generate professional OpenAPI documentation, and set up contract testing for production-ready APIs. 