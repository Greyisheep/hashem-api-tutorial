# Day 1 Take-Home Assignment: Contribute to TaskFlow API

## 🎯 Objective
Extend the TaskFlow API with new functionality and contribute to the collaborative repository. This assignment reinforces Day 1 concepts while building real-world collaboration skills.

## 📋 Assignment Overview

### Part 1: Individual Contribution (60 minutes)
Each student will add a new domain to the TaskFlow API:

**Choose ONE of these domains:**
- **Users Domain**: User management and profiles
- **Comments Domain**: Task comments and discussions  
- **Notifications Domain**: System notifications and alerts
- **Attachments Domain**: File attachments for tasks
- **Tags Domain**: Task categorization and tagging

### Part 2: Collaborative Review (30 minutes)
- Review and provide feedback on peer contributions
- Merge approved pull requests
- Update documentation

---

## 🚀 Getting Started

### 1. Fork the Repository
```bash
# Fork the main repository to your GitHub account
# Clone your fork locally
git clone https://github.com/YOUR_USERNAME/hashem-api-tutorial.git
cd hashem-api-tutorial/taskflow-api
```

### 2. Create Feature Branch
```bash
git checkout -b feature/add-[your-domain]-domain
# Example: feature/add-users-domain
```

### 3. Set Up Development Environment
```bash
# Start the API
docker-compose up --build

# Verify it's working
curl http://localhost:8001/health
```

---

## 📝 Implementation Requirements

### For Each Domain, Implement:

#### 1. **Data Models** (Pydantic)
```python
# Example for Users domain
class User(BaseModel):
    id: str
    username: str
    email: str
    full_name: str
    role: str
    created_at: str
    is_active: bool

class CreateUserRequest(BaseModel):
    username: str
    email: str
    full_name: str
    role: str = "user"

# User Story model (required for all domains)
class UserStory(BaseModel):
    id: str
    as_a: str
    i_want: str
    so_that: str
    acceptance_criteria: List[str]
    priority: str = "medium"
```

#### 2. **CRUD Endpoints** (Use Envelope Pattern)
- `GET /[domain]` - List all items
- `GET /[domain]/{id}` - Get specific item
- `POST /[domain]` - Create new item
- `PUT /[domain]/{id}` - Update item
- `DELETE /[domain]/{id}` - Delete item

**All responses must use the envelope pattern:**
```python
return ApiResponse(
    success=True,
    data=your_data,
    message="Operation completed successfully",
    timestamp=datetime.now().isoformat()
)
```

#### 3. **Error Handling** (Use Envelope Pattern)
- 404 for not found
- 400 for invalid data
- 409 for conflicts (e.g., duplicate username)
- Proper error messages with envelope structure

**Error responses must use envelope pattern:**
```python
raise HTTPException(
    status_code=404,
    detail={
        "success": False,
        "error": "USER_NOT_FOUND",
        "message": f"User with ID {user_id} not found",
        "timestamp": datetime.now().isoformat(),
        "version": "1.0.0"
    }
)
```

#### 4. **Sample Data**
Add 2-3 sample items to demonstrate functionality.

#### 5. **User Stories** (Required)
Create at least 2 user stories for your domain:
```python
user_stories_db = {
    "story_001": {
        "id": "story_001",
        "as_a": "user role",
        "i_want": "specific functionality",
        "so_that": "business value",
        "acceptance_criteria": ["criteria 1", "criteria 2"],
        "priority": "high/medium/low"
    }
}
```

---

## 🎨 Domain-Specific Requirements

### Users Domain
```python
# Endpoints: /users, /users/{user_id}
# Features: Username uniqueness, role-based access
# Sample data: admin user, regular user, inactive user
```

### Comments Domain
```python
# Endpoints: /comments, /comments/{comment_id}, /tasks/{task_id}/comments
# Features: Comment threading, author association
# Sample data: Comments on existing tasks
```

### Notifications Domain
```python
# Endpoints: /notifications, /notifications/{id}, /users/{user_id}/notifications
# Features: Read/unread status, notification types
# Sample data: Task assignments, due date reminders
```

### Attachments Domain
```python
# Endpoints: /attachments, /attachments/{id}, /tasks/{task_id}/attachments
# Features: File metadata, task association
# Sample data: Document links, image references
```

### Tags Domain
```python
# Endpoints: /tags, /tags/{id}, /tasks/{task_id}/tags
# Features: Tag colors, task categorization
# Sample data: Priority tags, category tags
```

---

## 🧪 Testing Requirements

### 1. **Manual Testing**
- Test all CRUD operations with Postman
- Verify error responses
- Check API documentation at `/docs`

### 2. **Integration Testing**
- Ensure new endpoints work with existing task endpoints
- Test cross-domain relationships (if applicable)

### 3. **Documentation Testing**
- Verify OpenAPI docs are generated correctly
- Test example requests in Swagger UI

---

## 📤 Submission Process

### 1. **Commit Your Changes**
```bash
git add .
git commit -m "feat: add [domain] domain with CRUD operations

- Add [domain] data models and endpoints
- Implement proper error handling
- Add sample data for testing
- Update API documentation"
```

### 2. **Push and Create PR**
```bash
git push origin feature/add-[your-domain]-domain
# Create Pull Request on GitHub
```

### 3. **PR Description Template**
```markdown
## 🎯 Domain Added
[Your chosen domain]

## 🚀 Features Implemented
- [ ] CRUD endpoints for [domain]
- [ ] Proper error handling
- [ ] Sample data included
- [ ] API documentation updated

## 🧪 Testing
- [ ] All endpoints tested with Postman
- [ ] Error scenarios verified
- [ ] Documentation reviewed

## 📝 Notes
[Any additional notes or questions]
```

---

## 👥 Collaborative Review Process

### 1. **Review Peers' PRs**
- Check code quality and structure
- Verify API design principles
- Test endpoints if needed
- Provide constructive feedback

### 2. **Merge Approved PRs**
- Only merge after thorough review
- Ensure no conflicts with existing code
- Update main branch documentation

### 3. **Documentation Updates**
- Update README.md with new endpoints
- Add examples to Postman collection
- Document any new patterns or conventions

---

## ✅ Success Criteria

### Individual Success:
- [ ] All CRUD endpoints working
- [ ] Proper error handling implemented
- [ ] Code follows FastAPI best practices
- [ ] PR submitted with clear description
- [ ] All tests passing

### Collaborative Success:
- [ ] Reviewed at least 2 peer PRs
- [ ] Provided constructive feedback
- [ ] Merged at least 1 approved PR
- [ ] Updated documentation

---

## 🎓 Learning Outcomes

By completing this assignment, you will have:
- ✅ Applied Day 1 API design principles
- ✅ Gained hands-on FastAPI experience
- ✅ Practiced real-world collaboration
- ✅ Contributed to a growing codebase
- ✅ Experienced code review process

---

## 🆘 Getting Help

### Common Issues:
- **Docker issues**: Check Docker Desktop is running
- **Git conflicts**: Use `git pull origin main` to sync
- **API errors**: Check logs with `docker-compose logs`

### Resources:
- [FastAPI Documentation](https://fastapi.tiangolo.com/)
- [Pydantic Models](https://pydantic-docs.helpmanual.io/)
- [HTTP Status Codes](https://httpstatuses.com/)

### Ask for Help:
- Create GitHub Issues for technical problems
- Use PR comments for code-specific questions
- Reach out to instructor for clarification

---

**Remember**: This is collaborative learning! Help each other, share knowledge, and build something amazing together. 🚀 