# gRPC Demo Materials

## TaskFlow Analytics Service Example

### Protocol Buffer Definition:
```protobuf
// taskflow_analytics.proto
syntax = "proto3";

package taskflow.analytics;

service TaskAnalytics {
  // Unary RPC - get team performance
  rpc GetTeamPerformance(TeamRequest) returns (TeamPerformance);
  
  // Server streaming - real-time task updates
  rpc StreamTaskUpdates(TaskStreamRequest) returns (stream TaskUpdate);
  
  // Client streaming - batch task updates
  rpc BatchUpdateTasks(stream TaskUpdateRequest) returns (BatchResponse);
  
  // Bidirectional streaming - real-time collaboration
  rpc CollaborateOnTask(stream TaskCollaboration) returns (stream TaskCollaboration);
}

message TeamRequest {
  string team_id = 1;
  int32 days_back = 2;
}

message TeamPerformance {
  string team_id = 1;
  int32 completed_tasks = 2;
  int32 overdue_tasks = 3;
  float velocity = 4;
  repeated TaskMetric task_metrics = 5;
}

message TaskUpdate {
  string task_id = 1;
  string status = 2;
  string assigned_to = 3;
  int64 timestamp = 4;
}

message TaskMetric {
  string task_id = 1;
  string title = 2;
  int32 days_to_complete = 3;
  string complexity = 4;
}

message TaskStreamRequest {
  string team_id = 1;
  repeated string status_filter = 2;
}

message TaskUpdateRequest {
  string task_id = 1;
  string new_status = 2;
  string user_id = 3;
}

message BatchResponse {
  int32 successful_updates = 1;
  int32 failed_updates = 2;
  repeated string error_messages = 3;
}

message TaskCollaboration {
  string task_id = 1;
  string user_id = 2;
  string action = 3;
  string data = 4;
  int64 timestamp = 5;
}
```

---

## Live Performance Comparison Demo

### Scenario: Get team performance data

#### REST Approach (Multiple Round Trips):
```bash
# Call 1: Get completed tasks
GET /teams/eng/tasks?status=completed&days=30
Response time: ~120ms

# Call 2: Get overdue tasks  
GET /teams/eng/tasks?status=overdue
Response time: ~110ms

# Call 3: Get velocity metrics
GET /teams/eng/velocity?days=30
Response time: ~95ms

# Call 4: Get team members
GET /teams/eng/members
Response time: ~85ms

Total: 4 requests, ~410ms, ~15KB of JSON
```

#### gRPC Approach (Single Call):
```bash
# Single call with structured response
grpc_cli call localhost:50051 GetTeamPerformance "team_id: 'eng', days_back: 30"

Response time: ~75ms
Total: 1 request, ~75ms, ~3KB binary data
```

---

## Key Differences Demonstration

### 1. Network Efficiency
**REST**: 4 HTTP/1.1 connections, JSON parsing overhead
**gRPC**: 1 HTTP/2 multiplexed connection, binary protobuf

### 2. Type Safety
**REST**: 
```javascript
// Runtime error possible
const velocity = response.velocity.toFixed(2); // What if velocity is null?
```

**gRPC**: 
```javascript
// Compile-time validation
const velocity = response.getVelocity(); // Type guaranteed by protobuf
```

### 3. Streaming Capabilities

#### REST Real-time (Polling):
```javascript
// Client must poll for updates
setInterval(() => {
  fetch('/teams/eng/tasks')
    .then(response => response.json())
    .then(updateUI);
}, 5000); // Poll every 5 seconds
```

#### gRPC Real-time (Streaming):
```javascript
// Server pushes updates as they happen
const stream = client.streamTaskUpdates({team_id: 'eng'});
stream.on('data', (taskUpdate) => {
  updateUI(taskUpdate); // Real-time update
});
```

### 4. Code Generation
**REST**: Manual client implementation
```javascript
class TaskFlowClient {
  async getTeamTasks(teamId) {
    // Manual implementation
    const response = await fetch(`/teams/${teamId}/tasks`);
    return response.json();
  }
}
```

**gRPC**: Generated client from .proto
```javascript
// Auto-generated from protobuf
const client = new TaskAnalyticsClient('localhost:50051');
const performance = await client.getTeamPerformance({team_id: 'eng', days_back: 30});
```

---

## When to Choose Each Protocol

### Choose REST When:
- **Public APIs** - Web browsers, third-party developers
- **Simple CRUD operations** - Standard create, read, update, delete
- **Human-readable APIs** - Debugging, documentation, learning
- **Web application frontends** - Direct browser consumption
- **Stateless operations** - Each request is independent

### Choose gRPC When:
- **Internal microservices** - Service-to-service communication
- **Real-time features** - Live updates, streaming data
- **High performance required** - Low latency, high throughput
- **Type safety important** - Strong contracts, fewer runtime errors
- **Polyglot environments** - Multiple programming languages

---

## Demo Script for Instructor

### Opening (2 min):
"REST is great for many use cases, but what if TaskFlow needed real-time collaboration? What if 50 microservices needed to communicate efficiently? Let me show you gRPC."

### Show .proto file (3 min):
- "This is our contract - both client and server are generated from this"
- "Notice the streaming capabilities - server can push data to clients"
- "Type safety is built-in - no more runtime JSON parsing errors"

### Performance demo (5 min):
- "Let's compare getting team performance data"
- Run both REST and gRPC calls side by side
- "Notice the difference in response time and payload size"

### Streaming demo (3 min):
- "Here's the real power - watch real-time task updates"
- Start gRPC stream, simulate task updates
- "No polling, no missed updates, no delays"

### Decision framework (2 min):
- "So when would you choose each?"
- Show decision matrix
- "It's about constraints, not preferences"

---

## Student Activity: Protocol Design Challenge

### Challenge:
"Design a protocol buffer for TaskFlow notifications that supports:
1. Sending notifications to users
2. Real-time delivery
3. Different notification types (task assigned, deadline approaching, etc.)
4. Delivery confirmation"

### Expected Solution:
```protobuf
service NotificationService {
  rpc SendNotification(NotificationRequest) returns (NotificationResponse);
  rpc StreamNotifications(UserRequest) returns (stream Notification);
  rpc ConfirmDelivery(DeliveryConfirmation) returns (ConfirmationResponse);
}

message NotificationRequest {
  string user_id = 1;
  NotificationType type = 2;
  string title = 3;
  string message = 4;
  map<string, string> metadata = 5;
}

enum NotificationType {
  TASK_ASSIGNED = 0;
  DEADLINE_APPROACHING = 1;
  TASK_COMPLETED = 2;
  TEAM_INVITATION = 3;
}
```

---

## Common Questions & Answers

### Q: "Can browsers use gRPC directly?"
**A**: "Not natively. You need gRPC-Web or a proxy. That's why public APIs typically use REST - browsers understand it natively."

### Q: "Is gRPC always faster than REST?"
**A**: "Not always. For simple, infrequent requests, the overhead might not be worth it. gRPC shines with frequent communication or streaming data."

### Q: "How do you debug gRPC calls?"
**A**: "Tools like grpcurl and Postman now support gRPC. But yes, it's less human-readable than JSON over HTTP."

### Q: "What about versioning with gRPC?"
**A**: "Protobuf has excellent backward compatibility. You can add fields without breaking existing clients. For breaking changes, you version the service."

---

**Demo Duration**: 15 minutes
**Key Takeaway**: Protocol choice depends on use case constraints, not personal preference 