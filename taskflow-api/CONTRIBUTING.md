# Contributing to TaskFlow API

Welcome to the TaskFlow API collaborative learning project! This guide explains how to contribute to the API as part of your learning journey.

## 🎯 Learning Objectives

By contributing to this repository, you will:
- ✅ Apply API design principles in practice
- ✅ Gain hands-on FastAPI experience
- ✅ Learn real-world collaboration workflows
- ✅ Build a portfolio-worthy project
- ✅ Experience code review processes

## 🚀 Quick Start

### 1. Fork the Repository
```bash
# Fork the main repository to your GitHub account
# Then clone your fork
git clone https://github.com/YOUR_USERNAME/hashem-api-tutorial.git
cd hashem-api-tutorial/taskflow-api
```

### 2. Set Up Development Environment
```bash
# Start the API
docker-compose up --build

# Verify it's working
curl http://localhost:8000/health
```

### 3. Create a Feature Branch
```bash
git checkout -b feature/add-[your-domain]-domain
# Example: feature/add-users-domain
```

## 📝 Contribution Guidelines

### Code Style
- Follow PEP 8 for Python code
- Use meaningful variable and function names
- Add docstrings to all functions
- Keep functions small and focused

### API Design Principles
- Use business language in URLs and responses
- Follow REST conventions
- Implement proper error handling
- Add comprehensive documentation

### Commit Messages
Use conventional commit format:
```
feat: add users domain with CRUD operations

- Add user data models and endpoints
- Implement proper error handling
- Add sample data for testing
- Update API documentation
```

## 🎨 Domain Implementation Template

### 1. Data Models (Pydantic)
```python
from pydantic import BaseModel
from typing import Optional
from datetime import datetime

class YourDomain(BaseModel):
    id: str
    name: str
    description: Optional[str] = None
    created_at: str
    updated_at: str

class CreateYourDomainRequest(BaseModel):
    name: str
    description: Optional[str] = None
```

### 2. CRUD Endpoints
```python
@app.get("/your-domain", response_model=List[YourDomain])
async def get_all_items():
    """Get all items"""
    return list(your_domain_db.values())

@app.get("/your-domain/{item_id}", response_model=YourDomain)
async def get_item(item_id: str):
    """Get a specific item by ID"""
    if item_id not in your_domain_db:
        raise HTTPException(status_code=404, detail="Item not found")
    return your_domain_db[item_id]

@app.post("/your-domain", response_model=YourDomain, status_code=201)
async def create_item(item_request: CreateYourDomainRequest):
    """Create a new item"""
    item_id = f"item_{len(your_domain_db) + 1:03d}"
    
    new_item = YourDomain(
        id=item_id,
        name=item_request.name,
        description=item_request.description,
        created_at=datetime.now().isoformat(),
        updated_at=datetime.now().isoformat()
    )
    
    your_domain_db[item_id] = new_item.dict()
    return new_item

@app.put("/your-domain/{item_id}", response_model=YourDomain)
async def update_item(item_id: str, item_update: dict):
    """Update an item"""
    if item_id not in your_domain_db:
        raise HTTPException(status_code=404, detail="Item not found")
    
    for key, value in item_update.items():
        if key in your_domain_db[item_id]:
            your_domain_db[item_id][key] = value
    
    your_domain_db[item_id]["updated_at"] = datetime.now().isoformat()
    return your_domain_db[item_id]

@app.delete("/your-domain/{item_id}")
async def delete_item(item_id: str):
    """Delete an item"""
    if item_id not in your_domain_db:
        raise HTTPException(status_code=404, detail="Item not found")
    
    del your_domain_db[item_id]
    return {"message": "Item deleted successfully"}
```

### 3. Sample Data
```python
# Add to your domain section
your_domain_db = {
    "item_001": {
        "id": "item_001",
        "name": "Sample Item 1",
        "description": "This is a sample item",
        "created_at": "2024-01-15T10:00:00Z",
        "updated_at": "2024-01-15T10:00:00Z"
    },
    "item_002": {
        "id": "item_002",
        "name": "Sample Item 2",
        "description": "Another sample item",
        "created_at": "2024-01-15T11:00:00Z",
        "updated_at": "2024-01-15T11:00:00Z"
    }
}
```

## 🧪 Testing Your Contribution

### 1. Manual Testing
```bash
# Test all CRUD operations
curl http://localhost:8000/your-domain
curl http://localhost:8000/your-domain/item_001
curl -X POST http://localhost:8000/your-domain \
  -H "Content-Type: application/json" \
  -d '{"name": "Test Item", "description": "Testing"}'
```

### 2. API Documentation
- Visit http://localhost:8000/docs
- Verify your endpoints appear correctly
- Test the interactive documentation

### 3. Error Scenarios
- Test with invalid IDs (should return 404)
- Test with invalid data (should return 400)
- Test with missing required fields

## 📤 Submitting Your Contribution

### 1. Commit Your Changes
```bash
git add .
git commit -m "feat: add [domain] domain with CRUD operations

- Add [domain] data models and endpoints
- Implement proper error handling
- Add sample data for testing
- Update API documentation"
```

### 2. Push and Create PR
```bash
git push origin feature/add-[your-domain]-domain
# Create Pull Request on GitHub
```

### 3. PR Description Template
```markdown
## 🎯 Domain Added
[Your chosen domain]

## 🚀 Features Implemented
- [ ] CRUD endpoints for [domain]
- [ ] Proper error handling
- [ ] Sample data included
- [ ] API documentation updated

## 🧪 Testing
- [ ] All endpoints tested with curl/Postman
- [ ] Error scenarios verified
- [ ] Documentation reviewed

## 📝 Notes
[Any additional notes or questions]
```

## 👥 Review Process

### As a Contributor
- Respond to review comments promptly
- Make requested changes
- Test your changes after updates
- Keep commits focused and clean

### As a Reviewer
- Check code quality and structure
- Verify API design principles
- Test endpoints if needed
- Provide constructive feedback
- Focus on learning, not perfection

## 🎓 Learning Outcomes

### Technical Skills
- FastAPI framework mastery
- REST API design principles
- Error handling best practices
- API documentation standards

### Collaboration Skills
- Git workflow proficiency
- Code review experience
- Team communication
- Project coordination

### Portfolio Building
- Real-world project contributions
- Open source experience
- API design portfolio
- Collaborative development examples

## 🆘 Getting Help

### Common Issues
- **Docker problems**: Check Docker Desktop is running
- **Git conflicts**: Use `git pull origin main` to sync
- **API errors**: Check logs with `docker-compose logs`

### Resources
- [FastAPI Documentation](https://fastapi.tiangolo.com/)
- [Pydantic Models](https://pydantic-docs.helpmanual.io/)
- [HTTP Status Codes](https://httpstatuses.com/)
- [Conventional Commits](https://www.conventionalcommits.org/)

### Ask for Help
- Create GitHub Issues for technical problems
- Use PR comments for code-specific questions
- Reach out to instructor for clarification

## 🏆 Success Metrics

### Individual Success
- [ ] All CRUD endpoints working
- [ ] Proper error handling implemented
- [ ] Code follows best practices
- [ ] PR submitted with clear description
- [ ] All tests passing

### Collaborative Success
- [ ] Reviewed peer contributions
- [ ] Provided helpful feedback
- [ ] Merged approved PRs
- [ ] Updated documentation

---

**Remember**: This is collaborative learning! Help each other, share knowledge, and build something amazing together. Every contribution makes the API better and helps everyone learn. 🚀 