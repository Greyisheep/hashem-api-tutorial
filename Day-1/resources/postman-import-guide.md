# Postman Collection Import Guide

## 🚀 Quick Start (5 minutes)

### Step 1: Import Collection
1. **Open Postman Desktop**
2. **Click "Import"** (top left)
3. **Choose File** → Select [`postman-collection.json`](./postman-collection.json)
4. **Click "Import"**

### Step 2: Import Environment  
1. **Still in Import dialog** 
2. **Choose File** → Select [`postman-environment.json`](./postman-environment.json)
3. **Click "Import"**

### Step 3: Activate Environment
1. **Top right dropdown** → Select "TaskFlow Day 1 Environment"
2. **Verify** environment variables are loaded

---

## 🧪 Test Your Setup

### Quick Test - API Treasure Hunt:
1. **Expand** "🎯 API Treasure Hunt" folder
2. **Open** "1. JSONPlaceholder (Good Example)" 
3. **Click** "Get Single Post"
4. **Send** request
5. **Verify** you get JSON response with post data

### TaskFlow Demo Test:
1. **Expand** "🚀 TaskFlow Demo API" folder
2. **Open** "Teams Management"
3. **Click** "Get All Teams"
4. **Send** request
5. **Should see** mock response or connection error (expected if no server running)

---

## 📁 Collection Structure

```
Day 1: API Foundation & Design
├── 🎯 API Treasure Hunt
│   ├── 1. JSONPlaceholder (Good Example)
│   │   ├── Get Single Post
│   │   ├── Get All Posts
│   │   ├── Get User Info
│   │   └── Create Post (Test)
│   ├── 2. GitHub API (Excellent Example)
│   │   ├── Get User - Octocat
│   │   ├── Get Repository
│   │   └── Get API Rate Limits
│   ├── 3. OpenWeather (Commercial Example)
│   │   ├── Get Weather by City
│   │   └── Get Weather by Coordinates
│   ├── 4. REST Countries (Simple Example)
│   │   ├── Get Country by Name
│   │   ├── Get Countries by Region
│   │   └── Get All Countries
│   └── 5. HTTPBin (Testing Tool)
│       ├── Test GET with parameters
│       ├── Test POST with JSON
│       └── Test Status Codes
└── 🚀 TaskFlow Demo API
    ├── Teams Management
    │   ├── Get All Teams
    │   ├── Get Engineering Team
    │   ├── Get Team Projects
    │   ├── Get Team Members
    │   └── Create New Team
    ├── Project Management
    │   ├── Get Project Details
    │   ├── Create Project in Team
    │   ├── Get Project Tasks
    │   └── Get Active Project Tasks
    ├── Task Management
    │   ├── Get Task Details
    │   ├── Create Task in Project
    │   ├── Assign Task to User
    │   ├── Update Task Status
    │   └── Add Task Comment
    └── 🧪 Status Code Scenarios
        ├── 404 - Project Not Found
        ├── 409 - Duplicate Team Name
        ├── 422 - Invalid Task Assignment
        └── 400 - Invalid JSON
```

---

## 🔧 Environment Variables

The environment includes these variables:

| Variable | Description | Example Value |
|----------|-------------|---------------|
| `taskflow_base_url` | Base URL for TaskFlow API | `https://taskflow-demo.herokuapp.com/api/v1` |
| `openweather_api_key` | API key for weather service | `demo_key_for_class` |
| `demo_user_token` | JWT token for auth demos | `eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.demo.token` |
| `team_id` | Default team for demos | `engineering` |
| `project_id` | Default project for demos | `website-redesign` |
| `task_id` | Default task for demos | `task_001` |

---

## 🎯 Using During Day 1

### API Treasure Hunt (10:15 AM):
- **Use folder**: "🎯 API Treasure Hunt"
- **Students explore**: Each of the 5 API examples
- **Look for**: Good vs bad API design patterns

### TaskFlow Demo (11:00 AM):
- **Use folder**: "🚀 TaskFlow Demo API" 
- **Show progression**: Teams → Projects → Tasks
- **Highlight**: Business-focused URLs and responses

### Status Code Scenarios (1:45 PM):
- **Use folder**: "🧪 Status Code Scenarios"
- **Demonstrate**: Different HTTP status codes
- **Show**: Professional error response patterns

---

## 🔧 Troubleshooting

### Collection not importing:
- **Check file path**: Make sure you're selecting the right JSON file
- **Try drag & drop**: Drag the JSON file directly into Postman
- **Manual import**: Copy JSON content and paste in Postman

### Environment not active:
- **Check dropdown**: Top right corner should show "TaskFlow Day 1 Environment"
- **Variables not working**: Click gear icon → check environment variables are set

### API calls failing:
- **Expected behavior**: TaskFlow API calls will fail without running server
- **Use for demo**: Show connection errors, then explain mock server setup
- **Treasure Hunt APIs**: Should work (external public APIs)

### OpenWeather API key needed:
- **Get free key**: https://openweathermap.org/api
- **Update environment**: Replace `demo_key_for_class` with real key
- **Or skip**: Use for demo of API key authentication

---

## 📋 Quick Reference

### For Students:
1. **Import both files** (collection + environment)
2. **Select environment** from dropdown
3. **Start with Treasure Hunt** folder
4. **Try different APIs** and compare responses

### For Instructors:
1. **Test all endpoints** before class
2. **Set up mock server** for TaskFlow demos (see [demo environment setup](../demos/demo-environment-setup.md))
3. **Have backup plan** (screenshots) if APIs are down
4. **Use variable substitution** (e.g., `{{taskflow_base_url}}`) in demos

---

**Ready to explore APIs!** 🎯 