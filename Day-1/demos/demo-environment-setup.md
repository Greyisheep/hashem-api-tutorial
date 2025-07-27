# Demo Environment Setup Guide

## 🎯 Overview

This guide helps you set up a working demo environment for Day 1's **TaskFlow API demonstrations**. You'll have a choice between using mock responses (quick setup) or running a real API server (more authentic).

---

## Option 1: Quick Setup with Mock Responses (Recommended)

### Using Postman Mock Server

#### Step 1: Import Collection & Environment
1. **Import Collection**: 
   - Open Postman
   - File → Import → [`resources/postman-collection.json`](../resources/postman-collection.json)
   
2. **Import Environment**:
   - Import → [`resources/postman-environment.json`](../resources/postman-environment.json)
   - Set as active environment

#### Step 2: Create Mock Server
1. **In Postman**:
   - Right-click collection → "Mock Collection"
   - Name: "TaskFlow Demo Mock"
   - Environment: Select "TaskFlow Day 1 Environment"
   - Save mock URL

2. **Update Environment**:
   - Update `taskflow_base_url` to your mock server URL
   - Example: `https://12345678-1234-1234-1234-123456789012.mock.pstmn.io`

#### Step 3: Add Example Responses
For each request in the TaskFlow Demo API folder, add example responses:

**Get All Teams Example Response:**
```json
{
  "teams": [
    {
      "id": "engineering",
      "name": "Engineering Team", 
      "description": "Software development and architecture",
      "memberCount": 12,
      "projectCount": 5,
      "createdAt": "2024-01-01T10:00:00Z"
    },
    {
      "id": "design",
      "name": "Design Team",
      "description": "User experience and visual design", 
      "memberCount": 6,
      "projectCount": 3,
      "createdAt": "2024-01-02T10:00:00Z"
    }
  ],
  "meta": {
    "total": 2,
    "page": 1,
    "limit": 20
  }
}
```

**Get Team Projects Example Response:**
```json
{
  "projects": [
    {
      "id": "website-redesign",
      "name": "Website Redesign",
      "description": "Complete overhaul of company website",
      "status": "in_progress",
      "teamId": "engineering",
      "tasksCount": 12,
      "completedTasks": 8,
      "startDate": "2024-01-01",
      "dueDate": "2024-03-01"
    }
  ],
  "meta": {
    "total": 1,
    "filtered": true,
    "filters": {
      "status": "active"
    }
  }
}
```

**Create Task Success Response (201):**
```json
{
  "task": {
    "id": "task_001",
    "title": "Implement user authentication",
    "description": "Add JWT-based login system with role management", 
    "status": "todo",
    "priority": "high",
    "projectId": "website-redesign",
    "estimatedHours": 8,
    "createdAt": "2024-01-15T10:30:00Z"
  }
}
```

**Task Assignment Success Response (200):**
```json
{
  "task": {
    "id": "task_001",
    "title": "Implement user authentication",
    "status": "todo",
    "assignedTo": "jane.smith",
    "assignedBy": "john.doe",
    "assignedAt": "2024-01-15T10:35:00Z"
  }
}
```

**Error Responses for Status Code Demo:**

404 - Project Not Found:
```json
{
  "error": {
    "type": "not_found",
    "message": "Project 'nonexistent' does not exist",
    "details": {
      "resource": "project",
      "id": "nonexistent"
    },
    "suggestion": "Check the project ID and try again"
  }
}
```

409 - Duplicate Team Name:
```json
{
  "error": {
    "type": "conflict", 
    "message": "Team with name 'engineering' already exists",
    "details": {
      "field": "name",
      "value": "engineering"
    },
    "suggestion": "Choose a different team name"
  }
}
```

422 - Invalid Task Assignment:
```json
{
  "error": {
    "type": "invalid_request",
    "message": "Cannot assign task to user outside the team",
    "details": {
      "taskId": "task_001",
      "userId": "user.not.in.team",
      "teamId": "engineering"
    },
    "suggestion": "Add user to team first, then assign task"
  }
}
```

---

## Option 2: Real API Server (Advanced)

### Using FastAPI Mock Server

#### Step 1: Create Mock Server
Create a simple FastAPI server that returns the demo responses:

```python
# demo_server.py
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
import uvicorn

app = FastAPI(
    title="TaskFlow Demo API",
    description="Demo server for Day 1 API course",
    version="1.0.0"
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Sample data
teams_data = [
    {
        "id": "engineering",
        "name": "Engineering Team",
        "description": "Software development and architecture",
        "memberCount": 12,
        "projectCount": 5,
        "createdAt": "2024-01-01T10:00:00Z"
    },
    {
        "id": "design", 
        "name": "Design Team",
        "description": "User experience and visual design",
        "memberCount": 6,
        "projectCount": 3,
        "createdAt": "2024-01-02T10:00:00Z"
    }
]

@app.get("/teams")
async def get_teams():
    return {
        "teams": teams_data,
        "meta": {"total": len(teams_data), "page": 1, "limit": 20}
    }

@app.get("/teams/{team_id}")
async def get_team(team_id: str):
    team = next((t for t in teams_data if t["id"] == team_id), None)
    if not team:
        raise HTTPException(
            status_code=404,
            detail={
                "error": {
                    "type": "not_found",
                    "message": f"Team '{team_id}' does not exist",
                    "details": {"resource": "team", "id": team_id},
                    "suggestion": "Check the team ID and try again"
                }
            }
        )
    return {"team": team}

@app.get("/teams/{team_id}/projects")
async def get_team_projects(team_id: str, status: str = None):
    if team_id not in [t["id"] for t in teams_data]:
        raise HTTPException(status_code=404, detail="Team not found")
    
    projects = [
        {
            "id": "website-redesign",
            "name": "Website Redesign", 
            "description": "Complete overhaul of company website",
            "status": "in_progress",
            "teamId": team_id,
            "tasksCount": 12,
            "completedTasks": 8,
            "startDate": "2024-01-01",
            "dueDate": "2024-03-01"
        }
    ]
    
    if status:
        # Filter by status for demo
        filtered_projects = [p for p in projects if p["status"] == "in_progress"]
        return {
            "projects": filtered_projects,
            "meta": {"total": len(filtered_projects), "filtered": True, "filters": {"status": status}}
        }
    
    return {
        "projects": projects,
        "meta": {"total": len(projects), "filtered": False}
    }

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)
```

#### Step 2: Run Server
```bash
# Install dependencies
pip install fastapi uvicorn

# Run server
python demo_server.py
```

#### Step 3: Update Environment
- Update `taskflow_base_url` to `http://localhost:8000`

---

## Option 3: Cloud Deployment (Professional)

### Using Railway/Heroku

#### Quick Deploy to Railway:
1. **Create GitHub repo** with the FastAPI demo server
2. **Connect to Railway**:
   - Go to [railway.app](https://railway.app)
   - "Deploy from GitHub"
   - Select your repo
   - Railway will auto-deploy

3. **Get deployment URL** and update Postman environment

#### Alternative: Heroku
```bash
# Create Procfile
echo "web: uvicorn demo_server:app --host=0.0.0.0 --port=\$PORT" > Procfile

# Deploy to Heroku
heroku create taskflow-demo-api
git push heroku main
```

---

## 🧪 Demo Testing Checklist

Before Day 1, test these demo flows:

### API Treasure Hunt APIs:
- [ ] JSONPlaceholder: GET /posts/1 works
- [ ] GitHub API: GET /users/octocat works  
- [ ] REST Countries: GET /name/france works
- [ ] HTTPBin: GET /get works
- [ ] OpenWeather: GET /weather works (or shows auth error)

### TaskFlow Demo APIs:
- [ ] GET /teams returns team list
- [ ] GET /teams/engineering/projects returns projects
- [ ] POST /projects/website-redesign/tasks creates task
- [ ] PUT /tasks/task_001/assign assigns task
- [ ] Error scenarios return proper status codes

### Status Code Scenarios:
- [ ] 404: GET /projects/nonexistent returns 404
- [ ] 409: POST duplicate team returns 409
- [ ] 422: Invalid task assignment returns 422

---

## 📋 Demo Day Preparation

### 1 Hour Before Class:
- [ ] Start mock server or verify cloud deployment
- [ ] Test all demo endpoints in Postman
- [ ] Have backup screenshots ready
- [ ] Verify environment variables are set

### During Demo Sections:

#### Module 2A Demo (11:00 AM):
**Script**: "Let me show you TaskFlow's complete API..."
1. **Open Postman collection**
2. **Show folder structure** (Teams → Projects → Tasks)
3. **Run GET /teams** → Show business-focused response
4. **Run GET /teams/engineering/projects** → Highlight URL readability
5. **Compare** to ugly technical URLs

#### Module 2D Demo (2:45 PM):
**gRPC Comparison**: 
1. **Show REST approach**: Multiple API calls for team performance
2. **Show gRPC .proto file**: Single call, type safety
3. **Performance comparison**: "REST: 4 calls, 400ms vs gRPC: 1 call, 75ms"

---

## 🔧 Troubleshooting

### Common Issues:

**Mock server not responding:**
- Check URL in environment variables
- Verify mock server is running
- Use backup option (screenshots)

**CORS errors in browser:**
- Expected for some APIs (explain during demo)
- Use Postman instead of browser console

**Rate limiting (GitHub, OpenWeather):**
- Expected behavior, use as teaching moment
- Show error response format

### Backup Plans:
1. **Screenshots**: Pre-captured successful API calls
2. **Local JSON files**: Static responses in Postman
3. **Video recording**: Pre-recorded demo sessions

---

## 🎯 Success Metrics

**Demo is successful when:**
- [ ] Students see clean, business-focused API responses
- [ ] Status code scenarios work as expected  
- [ ] Error responses are professional and helpful
- [ ] URLs read like business conversations
- [ ] Performance differences are visible (gRPC demo)

**Ready to teach domain-driven API design!** 🚀 