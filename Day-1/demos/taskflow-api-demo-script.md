# TaskFlow API Demo Script (10 minutes)

## 1. Business Overview (2 min)

### Opening Statement:
"TaskFlow is a project management system for software teams. Let me show you the complete API and how we've designed it to speak business language, not database language."

### Demo Points:
- **Show**: Complete OpenAPI documentation with all endpoints
- **Highlight**: How business language appears in API design
- **Point out**: No technical jargon in URLs or responses

---

## 2. Domain Structure (3 min)

### Show File Structure:
```
taskflow/
├── teams/           # Team Management Domain
│   ├── endpoints/
│   ├── models/
│   └── services/
├── projects/        # Project Planning Domain  
│   ├── endpoints/
│   ├── models/
│   └── services/
├── tasks/          # Task Execution Domain
│   ├── endpoints/
│   ├── models/
│   └── services/
├── users/          # User Identity Domain
│   ├── endpoints/
│   ├── models/
│   └── services/
└── notifications/  # Communication Domain
    ├── endpoints/
    ├── models/
    └── services/
```

### Key Messages:
- "Each directory represents a business capability"
- "Notice how the structure mirrors how the business thinks about work"
- "No generic 'models' or 'controllers' - everything is domain-specific"

---

## 3. API Design Philosophy (3 min)

### Show Examples:

#### ✅ Business-Focused URLs:
```
GET  /teams/engineering/projects
POST /teams/engineering/projects
GET  /projects/website-redesign/tasks
POST /projects/website-redesign/tasks
PUT  /tasks/implement-login/assign
POST /tasks/implement-login/comments
```

#### ❌ Technical-Focused URLs (What NOT to do):
```
GET  /api/v1/user-project-assignments?filter=active
POST /api/v1/task-status-updates
GET  /api/v1/team-member-relationships
```

### Key Messages:
- "URLs should read like business conversations"
- "Error messages use business terms, not database errors"
- "A product manager should understand our API documentation"

---

## 4. Real Endpoints (2 min)

### Live Demo Calls:

#### 1. Get Team's Projects:
```bash
GET /teams/engineering/projects

Response:
{
  "projects": [
    {
      "id": "proj_001",
      "name": "Website Redesign",
      "status": "in_progress",
      "teamId": "engineering",
      "tasksCount": 12,
      "completedTasks": 8
    }
  ]
}
```

#### 2. Create New Task:
```bash
POST /projects/website-redesign/tasks
Content-Type: application/json

{
  "title": "Implement user authentication",
  "description": "Add JWT-based login system",
  "priority": "high",
  "assignedTo": "john.doe"
}

Response: 201 Created
{
  "task": {
    "id": "task_001",
    "title": "Implement user authentication",
    "status": "todo",
    "createdAt": "2024-01-15T10:30:00Z",
    "project": "website-redesign"
  }
}
```

#### 3. Assign Task:
```bash
PUT /tasks/task_001/assign
Content-Type: application/json

{
  "assignedTo": "jane.smith"
}

Response: 200 OK
{
  "task": {
    "id": "task_001",
    "assignedTo": "jane.smith",
    "assignedAt": "2024-01-15T10:35:00Z"
  }
}
```

---

## Demo Tips for Instructor:

### Before Demo:
- [ ] Have Postman collection ready with working requests
- [ ] Prepare sample data in demo database
- [ ] Test all endpoints are responding correctly
- [ ] Have backup screenshots if API is down

### During Demo:
- **Keep it conversational**: "Notice how..."
- **Pause for questions**: "What do you think about this approach?"
- **Compare alternatives**: "This could also be designed as..."
- **Highlight business language**: Point out readable URLs

### After Demo:
- **Ask for reactions**: "What stands out to you?"
- **Connect to their experience**: "How is this different from APIs you've used?"
- **Preview next activity**: "Now you'll design similar endpoints..."

---

## Common Student Questions & Answers:

### Q: "Why not just use `/projects?team=engineering`?"
**A**: "Great question! Nested resources show the relationship clearly. `/teams/engineering/projects` reads like business language: 'Show me the engineering team's projects.' Query parameters feel more like filtering than relationships."

### Q: "What if a task belongs to multiple projects?"
**A**: "Excellent edge case! That would suggest our domain model needs refinement. Maybe we need a different entity like 'Epic' that spans projects, or we accept that tasks belong to one project but can reference others."

### Q: "How do you handle pagination on these nested endpoints?"
**A**: "Standard query parameters: `/teams/engineering/projects?page=1&limit=20`. The business relationship stays in the URL, pagination stays in query params."

### Q: "What about performance with all these nested calls?"
**A**: "Good thinking! That's where GraphQL shines for frontend needs, or we provide flattened endpoints for specific use cases. Always design for clarity first, optimize for performance second."

---

## Connection to Next Activity:

"Now you've seen TaskFlow's complete design. In the next 20 minutes, you'll model the domains yourself using sticky notes and identify the same boundaries we discovered. Ready to get hands-on?"

---

**Demo Duration**: 10 minutes maximum
**Energy Level**: High enthusiasm, conversational
**Outcome**: Students understand domain-driven API design principles 