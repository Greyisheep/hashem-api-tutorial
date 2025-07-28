from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import List, Optional, Generic, TypeVar, Any
import uvicorn
from datetime import datetime

app = FastAPI(
    title="Day 1 API Learning",
    description="Simple API for learning REST principles, FastAPI, and response patterns",
    version="1.0.0"
)

# Enable CORS for frontend development
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Envelope Pattern Implementation
T = TypeVar('T')

class ApiResponse(BaseModel, Generic[T]):
    """Envelope pattern for consistent API responses"""
    success: bool
    data: Optional[T] = None
    message: Optional[str] = None
    timestamp: str
    version: str = "1.0.0"

class ErrorResponse(BaseModel):
    """Standardized error response"""
    success: bool = False
    error: str
    message: str
    timestamp: str
    version: str = "1.0.0"

# Data models
class HealthResponse(BaseModel):
    status: str
    timestamp: str
    version: str

class Task(BaseModel):
    id: str
    title: str
    description: str
    status: str
    created_at: str
    user_story: Optional[str] = None  # User story context

class CreateTaskRequest(BaseModel):
    title: str
    description: str
    user_story: Optional[str] = None

class UserStory(BaseModel):
    """User story model for business context"""
    id: str
    as_a: str
    i_want: str
    so_that: str
    acceptance_criteria: List[str]
    priority: str = "medium"

# In-memory storage (for demo purposes)
tasks_db = {
    "task_001": {
        "id": "task_001",
        "title": "Learn FastAPI",
        "description": "Build your first API with FastAPI",
        "status": "in_progress",
        "created_at": "2024-01-15T10:00:00Z",
        "user_story": "As a developer, I want to learn FastAPI so that I can build modern APIs quickly"
    },
    "task_002": {
        "id": "task_002", 
        "title": "Understand REST",
        "description": "Learn REST principles and HTTP methods",
        "status": "completed",
        "created_at": "2024-01-14T09:00:00Z",
        "user_story": "As a developer, I want to understand REST principles so that I can design better APIs"
    }
}

user_stories_db = {
    "story_001": {
        "id": "story_001",
        "as_a": "project manager",
        "i_want": "to see all tasks for my team",
        "so_that": "I can track progress and identify blockers",
        "acceptance_criteria": [
            "Can view all tasks in a project",
            "Can filter by status",
            "Can see task assignments"
        ],
        "priority": "high"
    },
    "story_002": {
        "id": "story_002",
        "as_a": "developer",
        "i_want": "to update task status",
        "so_that": "I can keep the team informed of my progress",
        "acceptance_criteria": [
            "Can change status to in_progress",
            "Can mark tasks as completed",
            "Can add comments to tasks"
        ],
        "priority": "medium"
    }
}

@app.get("/", response_model=ApiResponse[dict])
async def root():
    """Root endpoint with API information"""
    return ApiResponse(
        success=True,
        data={
            "message": "Welcome to Day 1 API Learning!",
            "version": "1.0.0",
            "endpoints": {
                "health": "/health",
                "tasks": "/tasks",
                "user_stories": "/user-stories",
                "docs": "/docs"
            }
        },
        message="API is running successfully",
        timestamp=datetime.now().isoformat()
    )

@app.get("/health", response_model=ApiResponse[HealthResponse])
async def health_check():
    """Health check endpoint"""
    return ApiResponse(
        success=True,
        data=HealthResponse(
            status="healthy",
            timestamp=datetime.now().isoformat(),
            version="1.0.0"
        ),
        message="Service is healthy",
        timestamp=datetime.now().isoformat()
    )

@app.get("/tasks", response_model=ApiResponse[List[Task]])
async def get_tasks():
    """Get all tasks - demonstrates envelope pattern"""
    return ApiResponse(
        success=True,
        data=list(tasks_db.values()),
        message="Tasks retrieved successfully",
        timestamp=datetime.now().isoformat()
    )

@app.get("/tasks/{task_id}", response_model=ApiResponse[Task])
async def get_task(task_id: str):
    """Get a specific task by ID"""
    if task_id not in tasks_db:
        raise HTTPException(
            status_code=404, 
            detail={
                "success": False,
                "error": "TASK_NOT_FOUND",
                "message": f"Task with ID {task_id} not found",
                "timestamp": datetime.now().isoformat(),
                "version": "1.0.0"
            }
        )
    
    return ApiResponse(
        success=True,
        data=tasks_db[task_id],
        message="Task retrieved successfully",
        timestamp=datetime.now().isoformat()
    )

@app.post("/tasks", response_model=ApiResponse[Task], status_code=201)
async def create_task(task_request: CreateTaskRequest):
    """Create a new task - includes user story context"""
    task_id = f"task_{len(tasks_db) + 1:03d}"
    
    new_task = Task(
        id=task_id,
        title=task_request.title,
        description=task_request.description,
        status="pending",
        created_at=datetime.now().isoformat(),
        user_story=task_request.user_story
    )
    
    tasks_db[task_id] = new_task.dict()
    
    return ApiResponse(
        success=True,
        data=new_task,
        message="Task created successfully",
        timestamp=datetime.now().isoformat()
    )

@app.put("/tasks/{task_id}", response_model=ApiResponse[Task])
async def update_task(task_id: str, task_update: dict):
    """Update a task"""
    if task_id not in tasks_db:
        raise HTTPException(
            status_code=404, 
            detail={
                "success": False,
                "error": "TASK_NOT_FOUND",
                "message": f"Task with ID {task_id} not found",
                "timestamp": datetime.now().isoformat(),
                "version": "1.0.0"
            }
        )

    # Update only provided fields
    for key, value in task_update.items():
        if key in tasks_db[task_id]:
            tasks_db[task_id][key] = value

    return ApiResponse(
        success=True,
        data=tasks_db[task_id],
        message="Task updated successfully",
        timestamp=datetime.now().isoformat()
    )

@app.delete("/tasks/{task_id}", response_model=ApiResponse[dict])
async def delete_task(task_id: str):
    """Delete a task"""
    if task_id not in tasks_db:
        raise HTTPException(
            status_code=404, 
            detail={
                "success": False,
                "error": "TASK_NOT_FOUND",
                "message": f"Task with ID {task_id} not found",
                "timestamp": datetime.now().isoformat(),
                "version": "1.0.0"
            }
        )

    del tasks_db[task_id]
    return ApiResponse(
        success=True,
        data={"message": "Task deleted successfully"},
        message="Task deleted successfully",
        timestamp=datetime.now().isoformat()
    )

# User Stories endpoints
@app.get("/user-stories", response_model=ApiResponse[List[UserStory]])
async def get_user_stories():
    """Get all user stories - demonstrates business context"""
    return ApiResponse(
        success=True,
        data=list(user_stories_db.values()),
        message="User stories retrieved successfully",
        timestamp=datetime.now().isoformat()
    )

@app.get("/user-stories/{story_id}", response_model=ApiResponse[UserStory])
async def get_user_story(story_id: str):
    """Get a specific user story"""
    if story_id not in user_stories_db:
        raise HTTPException(
            status_code=404, 
            detail={
                "success": False,
                "error": "STORY_NOT_FOUND",
                "message": f"User story with ID {story_id} not found",
                "timestamp": datetime.now().isoformat(),
                "version": "1.0.0"
            }
        )
    
    return ApiResponse(
        success=True,
        data=user_stories_db[story_id],
        message="User story retrieved successfully",
        timestamp=datetime.now().isoformat()
    )

@app.post("/user-stories", response_model=ApiResponse[UserStory], status_code=201)
async def create_user_story(story: UserStory):
    """Create a new user story"""
    story_id = f"story_{len(user_stories_db) + 1:03d}"
    story.id = story_id
    
    user_stories_db[story_id] = story.dict()
    
    return ApiResponse(
        success=True,
        data=story,
        message="User story created successfully",
        timestamp=datetime.now().isoformat()
    )

# Response pattern examples
@app.get("/response-patterns/envelope", response_model=ApiResponse[dict])
async def demonstrate_envelope_pattern():
    """Demonstrate envelope pattern vs direct response"""
    return ApiResponse(
        success=True,
        data={
            "example": "This is wrapped in an envelope",
            "benefits": [
                "Consistent response structure",
                "Metadata included (timestamp, version)",
                "Clear success/failure indication",
                "Extensible for future needs"
            ]
        },
        message="Envelope pattern demonstration",
        timestamp=datetime.now().isoformat()
    )

@app.get("/response-patterns/direct")
async def demonstrate_direct_response():
    """Demonstrate direct response (no envelope)"""
    return {
        "example": "This is a direct response",
        "drawbacks": [
            "No consistent structure",
            "No metadata",
            "Harder to handle errors uniformly",
            "Less extensible"
        ]
    }

# Error demonstration endpoints with envelope pattern
@app.get("/error/400")
async def demonstrate_400():
    """Demonstrate 400 Bad Request with envelope pattern"""
    raise HTTPException(
        status_code=400, 
        detail={
            "success": False,
            "error": "BAD_REQUEST",
            "message": "Bad request - invalid parameters",
            "timestamp": datetime.now().isoformat(),
            "version": "1.0.0"
        }
    )

@app.get("/error/404")
async def demonstrate_404():
    """Demonstrate 404 Not Found with envelope pattern"""
    raise HTTPException(
        status_code=404, 
        detail={
            "success": False,
            "error": "NOT_FOUND",
            "message": "Resource not found",
            "timestamp": datetime.now().isoformat(),
            "version": "1.0.0"
        }
    )

@app.get("/error/500")
async def demonstrate_500():
    """Demonstrate 500 Internal Server Error with envelope pattern"""
    raise HTTPException(
        status_code=500, 
        detail={
            "success": False,
            "error": "INTERNAL_ERROR",
            "message": "Internal server error",
            "timestamp": datetime.now().isoformat(),
            "version": "1.0.0"
        }
    )

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000) 