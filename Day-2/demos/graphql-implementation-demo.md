# GraphQL Implementation Demo - Day 2

## 🎯 Demo Objectives
- Demonstrate GraphQL vs REST for complex data requirements
- Show over-fetching and under-fetching problems
- Implement GraphQL endpoint in .NET
- Understand when to choose GraphQL over REST

## ⏰ Demo Timing: 30 minutes

---

## 🚀 Setup Instructions

### Prerequisites
- .NET 8 SDK installed
- Hot Chocolate GraphQL server package
- GraphQL playground or GraphiQL

### Quick Start
```bash
# Create new .NET Web API project
dotnet new webapi -n TaskFlow.GraphQL

# Add GraphQL packages
cd TaskFlow.GraphQL
dotnet add package HotChocolate.AspNetCore
dotnet add package HotChocolate.Data.EntityFramework
```

---

## 📊 GraphQL vs REST Comparison Demo

### Step 1: REST API Data Fetching Problems (10 min)

#### Scenario: Task Management Dashboard
**User Story**: "As a project manager, I want to see a dashboard with project details, team members, and recent tasks so that I can track project progress."

#### REST API Endpoints
```csharp
// REST API endpoints needed
GET /api/projects/{id}                    // Project details
GET /api/projects/{id}/team              // Team members
GET /api/projects/{id}/tasks             // Project tasks
GET /api/users/{id}                      // User details (for each team member)
GET /api/tasks/{id}/assignee             // Task assignee details
GET /api/tasks/{id}/comments             // Task comments
```

#### REST API Problems Demonstration

**Problem 1: Over-fetching**
```bash
# Get project details - but we only need name and status
curl -X GET http://localhost:5000/api/projects/123
```

**Response includes unnecessary data:**
```json
{
  "id": "123",
  "name": "TaskFlow API",
  "description": "Building a comprehensive API...",  // Not needed
  "status": "active",
  "created_at": "2024-01-15T10:00:00Z",            // Not needed
  "updated_at": "2024-01-20T14:30:00Z",            // Not needed
  "owner_id": "user_456",                          // Not needed
  "settings": {                                    // Not needed
    "notifications": true,
    "privacy": "public"
  }
}
```

**Problem 2: Under-fetching**
```bash
# Need multiple requests to get complete dashboard data
curl -X GET http://localhost:5000/api/projects/123
curl -X GET http://localhost:5000/api/projects/123/team
curl -X GET http://localhost:5000/api/projects/123/tasks
# Plus additional requests for each team member and task details
```

**Total: 1 + 1 + 1 + N(team members) + M(tasks) = 3+N+M requests**

### Step 2: GraphQL Solution (10 min)

#### Single GraphQL Query
```graphql
query ProjectDashboard($projectId: ID!) {
  project(id: $projectId) {
    id
    name
    status
    team {
      id
      name
      email
      role
    }
    tasks(first: 5) {
      id
      title
      status
      assignee {
        name
        email
      }
      comments(first: 3) {
        id
        content
        author {
          name
        }
      }
    }
  }
}
```

#### GraphQL Benefits
1. **Single Request**: All data in one query
2. **Exact Data**: Only requested fields returned
3. **Flexible**: Frontend controls data shape
4. **Type Safe**: Compile-time validation

---

## 🔧 Building GraphQL in .NET

### Step 3: .NET GraphQL Implementation (10 min)

#### Project Structure
```
TaskFlow.GraphQL/
├── Models/
│   ├── Project.cs
│   ├── User.cs
│   ├── Task.cs
│   └── Comment.cs
├── GraphQL/
│   ├── Types/
│   ├── Queries/
│   └── Mutations/
├── Services/
└── Program.cs
```

#### Define Data Models
```csharp
// Models/Project.cs
public class Project
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public List<User> Team { get; set; } = new();
    public List<Task> Tasks { get; set; } = new();
}

// Models/User.cs
public class User
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<Task> AssignedTasks { get; set; } = new();
}

// Models/Task.cs
public class Task
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ProjectId { get; set; } = string.Empty;
    public string AssigneeId { get; set; } = string.Empty;
    public User? Assignee { get; set; }
    public List<Comment> Comments { get; set; } = new();
}

// Models/Comment.cs
public class Comment
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string TaskId { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public User? Author { get; set; }
}
```

#### Create GraphQL Types
```csharp
// GraphQL/Types/ProjectType.cs
public class ProjectType : ObjectType<Project>
{
    protected override void Configure(IObjectTypeDescriptor<Project> descriptor)
    {
        descriptor.Field(p => p.Id).Type<NonNullType<IdType>>();
        descriptor.Field(p => p.Name).Type<NonNullType<StringType>>();
        descriptor.Field(p => p.Status).Type<NonNullType<StringType>>();
        descriptor.Field(p => p.Team).ResolveWith<ProjectResolvers>(p => p.GetTeam(default!, default!));
        descriptor.Field(p => p.Tasks).ResolveWith<ProjectResolvers>(p => p.GetTasks(default!, default!));
    }
}

// GraphQL/Types/UserType.cs
public class UserType : ObjectType<User>
{
    protected override void Configure(IObjectTypeDescriptor<User> descriptor)
    {
        descriptor.Field(u => u.Id).Type<NonNullType<IdType>>();
        descriptor.Field(u => u.Name).Type<NonNullType<StringType>>();
        descriptor.Field(u => u.Email).Type<NonNullType<StringType>>();
        descriptor.Field(u => u.Role).Type<NonNullType<StringType>>();
    }
}

// GraphQL/Types/TaskType.cs
public class TaskType : ObjectType<Task>
{
    protected override void Configure(IObjectTypeDescriptor<Task> descriptor)
    {
        descriptor.Field(t => t.Id).Type<NonNullType<IdType>>();
        descriptor.Field(t => t.Title).Type<NonNullType<StringType>>();
        descriptor.Field(t => t.Status).Type<NonNullType<StringType>>();
        descriptor.Field(t => t.Assignee).ResolveWith<TaskResolvers>(t => t.GetAssignee(default!, default!));
        descriptor.Field(t => t.Comments).ResolveWith<TaskResolvers>(t => t.GetComments(default!, default!));
    }
}
```

#### Create Resolvers
```csharp
// GraphQL/Resolvers/ProjectResolvers.cs
public class ProjectResolvers
{
    public async Task<List<User>> GetTeam(Project project, [Service] IUserService userService)
    {
        // Simulate database lookup
        return await userService.GetTeamMembersAsync(project.Id);
    }

    public async Task<List<Task>> GetTasks(Project project, [Service] ITaskService taskService)
    {
        // Simulate database lookup
        return await taskService.GetProjectTasksAsync(project.Id);
    }
}

// GraphQL/Resolvers/TaskResolvers.cs
public class TaskResolvers
{
    public async Task<User?> GetAssignee(Task task, [Service] IUserService userService)
    {
        if (string.IsNullOrEmpty(task.AssigneeId))
            return null;
        
        return await userService.GetUserAsync(task.AssigneeId);
    }

    public async Task<List<Comment>> GetComments(Task task, [Service] ICommentService commentService)
    {
        return await commentService.GetTaskCommentsAsync(task.Id);
    }
}
```

#### Create Query Type
```csharp
// GraphQL/Queries/Query.cs
public class Query
{
    public async Task<Project?> GetProject(string id, [Service] IProjectService projectService)
    {
        return await projectService.GetProjectAsync(id);
    }

    public async Task<List<Project>> GetProjects([Service] IProjectService projectService)
    {
        return await projectService.GetProjectsAsync();
    }

    public async Task<User?> GetUser(string id, [Service] IUserService userService)
    {
        return await userService.GetUserAsync(id);
    }

    public async Task<List<Task>> GetTasks([Service] ITaskService taskService)
    {
        return await taskService.GetTasksAsync();
    }
}
```

#### Create Mutation Type
```csharp
// GraphQL/Mutations/Mutation.cs
public class Mutation
{
    public async Task<Project> CreateProject(
        CreateProjectInput input,
        [Service] IProjectService projectService)
    {
        var project = new Project
        {
            Id = Guid.NewGuid().ToString(),
            Name = input.Name,
            Description = input.Description,
            Status = "active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            OwnerId = input.OwnerId
        };

        return await projectService.CreateProjectAsync(project);
    }

    public async Task<Task> CreateTask(
        CreateTaskInput input,
        [Service] ITaskService taskService)
    {
        var task = new Task
        {
            Id = Guid.NewGuid().ToString(),
            Title = input.Title,
            Description = input.Description,
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            ProjectId = input.ProjectId,
            AssigneeId = input.AssigneeId
        };

        return await taskService.CreateTaskAsync(task);
    }
}

// GraphQL/Mutations/Inputs/CreateProjectInput.cs
public record CreateProjectInput(string Name, string Description, string OwnerId);

// GraphQL/Mutations/Inputs/CreateTaskInput.cs
public record CreateTaskInput(string Title, string Description, string ProjectId, string? AssigneeId);
```

#### Configure GraphQL in Program.cs
```csharp
// Program.cs
using HotChocolate.AspNetCore;
using TaskFlow.GraphQL.GraphQL.Queries;
using TaskFlow.GraphQL.GraphQL.Mutations;
using TaskFlow.GraphQL.GraphQL.Types;
using TaskFlow.GraphQL.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ICommentService, CommentService>();

// Add GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddType<ProjectType>()
    .AddType<UserType>()
    .AddType<TaskType>()
    .AddType<CommentType>()
    .AddFiltering()
    .AddSorting()
    .AddProjections();

var app = builder.Build();

// Configure GraphQL endpoint
app.MapGraphQL();

// Optional: Add GraphQL playground
app.MapGraphQL("/graphql");

app.Run();
```

---

## 🧪 Testing the GraphQL API

### Step 4: GraphQL Playground Testing (5 min)

#### Start the Application
```bash
dotnet run
```

#### Access GraphQL Playground
Navigate to: `http://localhost:5000/graphql`

#### Test Queries

**Simple Project Query:**
```graphql
query {
  project(id: "123") {
    id
    name
    status
  }
}
```

**Complex Dashboard Query:**
```graphql
query ProjectDashboard($projectId: ID!) {
  project(id: $projectId) {
    id
    name
    status
    team {
      id
      name
      email
      role
    }
    tasks {
      id
      title
      status
      assignee {
        name
        email
      }
      comments {
        content
        author {
          name
        }
      }
    }
  }
}
```

**Variables:**
```json
{
  "projectId": "123"
}
```

**Create Project Mutation:**
```graphql
mutation CreateProject($input: CreateProjectInput!) {
  createProject(input: $input) {
    id
    name
    status
    createdAt
  }
}
```

**Variables:**
```json
{
  "input": {
    "name": "New Project",
    "description": "A new project created via GraphQL",
    "ownerId": "user_123"
  }
}
```

---

## 🎯 Key Learning Points

### When to Use GraphQL
1. **Complex Data Requirements**: Multiple related entities
2. **Mobile Optimization**: Reduce payload size
3. **Frontend-Driven Development**: Let frontend control data shape
4. **Real-time Updates**: Subscriptions for live data
5. **API Evolution**: Add fields without breaking changes

### When to Stick with REST
1. **Simple CRUD Operations**: Basic data operations
2. **Caching Requirements**: HTTP caching is more mature
3. **File Uploads**: Better support for binary data
4. **Legacy Integration**: Existing REST infrastructure
5. **Browser Limitations**: GraphQL tooling still evolving

### Performance Considerations
- **Query Complexity**: N+1 query problems
- **Caching**: More complex than REST
- **Payload Size**: Can be larger for simple queries
- **Learning Curve**: Steeper than REST

---

## 🔧 Troubleshooting

### Common Issues
1. **N+1 Queries**: Use DataLoader for batching
2. **Schema Validation**: Check field names and types
3. **Resolver Errors**: Add proper error handling
4. **Performance**: Monitor query complexity

### Debug Commands
```bash
# Check GraphQL schema
curl -X POST http://localhost:5000/graphql \
  -H "Content-Type: application/json" \
  -d '{"query": "query { __schema { types { name } } }"}'

# Test introspection
curl -X POST http://localhost:5000/graphql \
  -H "Content-Type: application/json" \
  -d '{"query": "query IntrospectionQuery { __schema { queryType { name } } }"}'
```

---

## 📚 Additional Resources

- [Hot Chocolate Documentation](https://chillicream.com/docs/hotchocolate)
- [GraphQL Official Documentation](https://graphql.org/)
- [GraphQL vs REST](https://graphql.org/learn/comparison-with-rest/)
- [GraphQL Best Practices](https://graphql.org/learn/best-practices/)

---

**Demo Success**: Students understand when to use GraphQL vs REST and can implement GraphQL APIs in .NET for complex data requirements. 