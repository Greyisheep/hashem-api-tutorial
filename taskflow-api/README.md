# Day 1 API Learning - FastAPI Implementation

A simple FastAPI application designed to teach API fundamentals and REST principles.

## Quick Start (Choose One Method)

### Option 1: Docker (Recommended)
```bash
# Build and run with Docker Compose
docker-compose up --build

# Or run in background
docker-compose up -d --build
```

### Option 2: Local Python Setup
```bash
# Create virtual environment
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate

# Install dependencies
pip install -r requirements.txt

# Run the API
python src/main.py
```

### Option 3: Direct uvicorn
```bash
pip install -r requirements.txt
uvicorn src.main:app --reload --host 0.0.0.0 --port 8000
```

### 3. Access the API
- **API Root**: http://localhost:8001
- **Interactive Docs**: http://localhost:8001/docs
- **Health Check**: http://localhost:8001/health

## Available Endpoints

### Basic Endpoints
- `GET /` - API information (envelope pattern)
- `GET /health` - Health check (envelope pattern)
- `GET /docs` - Interactive API documentation

### Task Management (CRUD Operations)
- `GET /tasks` - Get all tasks (envelope pattern)
- `GET /tasks/{task_id}` - Get specific task (envelope pattern)
- `POST /tasks` - Create new task (includes user story)
- `PUT /tasks/{task_id}` - Update task (envelope pattern)
- `DELETE /tasks/{task_id}` - Delete task (envelope pattern)

### User Stories (Business Context)
- `GET /user-stories` - Get all user stories
- `GET /user-stories/{story_id}` - Get specific user story
- `POST /user-stories` - Create new user story

### Response Patterns
- `GET /response-patterns/envelope` - Envelope pattern demonstration
- `GET /response-patterns/direct` - Direct response comparison

### Error Demonstration
- `GET /error/400` - Bad Request error (envelope pattern)
- `GET /error/404` - Not Found error (envelope pattern)
- `GET /error/500` - Internal Server Error (envelope pattern)

## Docker Management

```bash
# Start the API
docker-compose up --build

# Stop the API
docker-compose down

# View logs
docker-compose logs -f

# Rebuild after code changes
docker-compose up --build --force-recreate
```

## Testing with Postman

1. Import the Postman collection from `Day-1/resources/postman-collection.json`
2. Set the `local_api_url` variable to `http://localhost:8001`
3. Test the endpoints!

## Learning Objectives

This API demonstrates:
- REST principles and HTTP methods
- FastAPI framework basics
- Request/response handling with envelope pattern
- Error handling with proper status codes and consistent structure
- API documentation with OpenAPI/Swagger
- User stories integration for business context
- Response pattern best practices

## Next Steps

After running this API, try:
1. Exploring the interactive documentation at `/docs`
2. Testing all CRUD operations on tasks
3. Understanding different HTTP status codes
4. Examining the envelope pattern vs direct responses
5. Creating user stories and understanding business context
6. Comparing response patterns and their benefits 