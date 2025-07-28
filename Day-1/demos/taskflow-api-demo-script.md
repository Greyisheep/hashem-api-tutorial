# TaskFlow API Demo Script - Day 1

## 🎯 Demo Objectives
- Show a working API that students can immediately use
- Demonstrate business language in URLs and user stories
- Show envelope pattern for consistent responses
- Demonstrate proper error handling and status codes
- Introduce the collaborative development workflow

## 🚀 Setup (5 minutes before demo)

### 1. Start the API
```bash
cd taskflow-api
docker-compose up --build
```

### 2. Verify it's working
```bash
curl http://localhost:8001/health
# Should return envelope pattern response
```

### 3. Open browser tabs
- API Root: http://localhost:8001
- Interactive Docs: http://localhost:8001/docs
- Health Check: http://localhost:8001/health

---

## 🎪 Demo Flow (12 minutes)

### Opening (1 minute)
"Let's look at what we're building today. This is a real, working API that demonstrates business-focused design, user stories, and production-ready response patterns."

### 1. Show API Root & Response Patterns (2 minutes)
```bash
curl http://localhost:8001
```
**Key Point**: "Notice the envelope pattern - consistent structure with success indicator, data, message, and timestamp."

### 2. Show Response Pattern Comparison (2 minutes)
```bash
# Envelope pattern
curl http://localhost:8001/response-patterns/envelope

# Direct response
curl http://localhost:8001/response-patterns/direct
```
**Key Point**: "Envelope pattern provides consistency, metadata, and better error handling for clients."

### 3. Show Interactive Documentation (2 minutes)
Navigate to http://localhost:8001/docs

**Demonstrate**:
- Click "Try it out" on GET /tasks
- Show the envelope response structure
- Click "Try it out" on POST /tasks
- Show the request body schema with user story field
- Execute with sample data

**Key Point**: "This documentation is automatically generated from our code. API-first design!"

### 4. Show Business Language & User Stories (3 minutes)
```bash
# Get all tasks
curl http://localhost:8001/tasks

# Get specific task
curl http://localhost:8001/tasks/task_001

# Create new task with user story
curl -X POST http://localhost:8001/tasks \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Learn API Design", 
    "description": "Study REST principles",
    "user_story": "As a developer, I want to learn API design so that I can build better systems"
  }'

# Show user stories endpoint
curl http://localhost:8001/user-stories
```

**Key Point**: "Notice the URLs speak business language: `/tasks`, not `/api/v1/task_entities`. User stories connect technical work to business value."

### 5. Show Error Handling with Envelope (2 minutes)
```bash
# 404 - Task not found
curl http://localhost:8001/tasks/nonexistent

# 400 - Bad request demo
curl http://localhost:8001/error/400

# 500 - Server error demo
curl http://localhost:8001/error/500
```

**Key Point**: "Proper error handling with envelope pattern - consistent structure even for errors."

---

## 🎯 Key Messages to Reinforce

### 1. **Response Pattern Benefits**
- Envelope pattern provides consistency across all endpoints
- Metadata (timestamp, version) included automatically
- Success indicators make client code simpler
- Extensible for future needs like pagination, caching info

### 2. **Business Language & User Stories**
- URLs like `/tasks` not `/api/v1/task_entities`
- User stories follow standard format (As a... I want... So that...)
- Business context in every endpoint
- Acceptance criteria define success

### 3. **Developer Experience**
- Interactive documentation
- Consistent response format with envelope pattern
- Clear error messages with proper structure
- Easy to test and explore

### 4. **Production Ready**
- Health check endpoints
- Proper status codes with envelope pattern
- Docker containerization
- API versioning ready

### 5. **Collaborative Development**
- "You'll all be contributing to this API today"
- "We'll extend it with new domains"
- "You'll review each other's code"
- "This is how real teams work"

---

## 🚨 Troubleshooting

### If Docker fails:
```bash
# Check Docker is running
docker --version

# Try rebuilding
docker-compose down
docker-compose up --build
```

### If API doesn't start:
```bash
# Check logs
docker-compose logs

# Check port availability
netstat -an | grep 8001
```

### Backup plan:
- Use screenshots of working API
- Show pre-recorded demo video
- Use Postman collection as backup

---

## 🎬 Demo Script (Word-for-word)

### Opening
"Welcome to Day 1! Before we dive into theory, let's look at what we're actually building. This is a real, working API that demonstrates business-focused design, user stories, and production-ready response patterns."

### During Demo
"Notice the envelope pattern - every response has the same structure with success indicator, data, message, and timestamp. This makes client code much simpler."

"Look at how user stories connect technical work to business value. This is what makes APIs truly useful."

"See how we handle errors? Consistent envelope pattern even for errors. This is production-ready error handling."

### Closing
"This is your foundation. You'll be extending this API with new domains, adding features, and collaborating with your peers. Let's get started!"

---

## 📋 Post-Demo Checklist

- [ ] API is running and accessible
- [ ] Students can access http://localhost:8001/docs
- [ ] Postman collection is imported
- [ ] Students understand envelope pattern benefits
- [ ] Students see user story integration
- [ ] Students understand they'll be contributing
- [ ] Questions answered about setup
- [ ] Ready to move to hands-on activities

---

**Remember**: This demo sets the tone for the entire day. Show enthusiasm for the technology and confidence in the learning approach! 