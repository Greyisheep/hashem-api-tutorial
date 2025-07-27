# Domain-to-REST Mapping Challenge (15 minutes)

## Given: TaskFlow Domain Model

```
Team {
  id, name, members[]
  Projects[]
}

Project { 
  id, name, description, status
  Tasks[], Team
}

Task {
  id, title, description, status, assignee
  Project, Comments[]
}
```

## Your Challenge: Design REST endpoints

### Rules:
1. **Use domain language in URLs**
2. **Follow REST conventions**  
3. **Think about relationships**
4. **Consider real-world usage**

### Questions to Consider:
- How do you get all projects for a team?
- How do you assign a task to someone?
- How do you handle nested resources?
- What about bulk operations?

---

## Your Solutions:

### Teams Domain:
```
HTTP_METHOD /endpoint_path

GET    /________________________________
POST   /________________________________
GET    /________________________________
PUT    /________________________________
DELETE /________________________________
GET    /________________________________  (team members)
POST   /________________________________  (add member)
DELETE /________________________________  (remove member)
```

### Projects Domain:
```
HTTP_METHOD /endpoint_path

GET    /________________________________  (team's projects)
POST   /________________________________  (create project)
GET    /________________________________  (specific project)
PUT    /________________________________  (update project)
DELETE /________________________________  (delete project)
```

### Tasks Domain:
```
HTTP_METHOD /endpoint_path

GET    /________________________________  (project's tasks)
POST   /________________________________  (create task)
GET    /________________________________  (specific task)
PUT    /________________________________  (update task)
POST   /________________________________  (assign task)
POST   /________________________________  (add comment)
```

---

## Design Decisions:

### 1. Nested vs Flat Resources
**Question**: Should it be `/teams/123/projects` or `/projects?team=123`?

**Your Choice**: ________________________________

**Why**: ________________________________

### 2. Action Endpoints
**Question**: How do you handle non-CRUD operations like "assign task"?

**Your Approach**: ________________________________

**Example**: ________________________________

### 3. Bulk Operations
**Question**: How would you handle "assign multiple tasks to a user"?

**Your Design**: ________________________________

### 4. Cross-Domain Operations
**Question**: What if you need "all tasks assigned to user X across all projects"?

**Your Solution**: ________________________________

---

## Expected Solutions:
*(Don't peek until you've tried!)*

<details>
<summary>Click to reveal suggested endpoints</summary>

### Teams Domain:
```
GET    /teams                           # List all teams
POST   /teams                           # Create team
GET    /teams/{id}                      # Get specific team
PUT    /teams/{id}                      # Update team
DELETE /teams/{id}                      # Delete team
GET    /teams/{id}/members              # Get team members
POST   /teams/{id}/members              # Add member to team
DELETE /teams/{id}/members/{userId}     # Remove member from team
```

### Projects Domain:
```
GET    /teams/{teamId}/projects         # Get team's projects
POST   /teams/{teamId}/projects         # Create project in team
GET    /projects/{id}                   # Get specific project
PUT    /projects/{id}                   # Update project
DELETE /projects/{id}                   # Delete project
```

### Tasks Domain:
```
GET    /projects/{projectId}/tasks      # Get project's tasks
POST   /projects/{projectId}/tasks      # Create task in project
GET    /tasks/{id}                      # Get specific task
PUT    /tasks/{id}                      # Update task
POST   /tasks/{id}/assign               # Assign task to user
POST   /tasks/{id}/comments             # Add comment to task
```

### Advanced Endpoints:
```
GET    /users/{userId}/tasks            # Cross-domain: user's tasks
GET    /teams/{teamId}/tasks            # Cross-domain: team's all tasks
POST   /tasks/bulk-assign               # Bulk operation
GET    /projects/{id}/tasks/overdue     # Filtered sub-resources
```

</details>

---

## Validation Questions:

1. **Are your URLs readable by business stakeholders?**
   ```
   Yes/No: 
   Example URL that proves it:
   ```

2. **Do you follow REST conventions?**
   ```
   Collections (plural): Yes/No
   Resources (singular in path): Yes/No  
   HTTP verbs used correctly: Yes/No
   ```

3. **How do you handle errors?**
   ```
   What if team doesn't exist in: POST /teams/999/projects?
   Status code: ____________
   Response:
   ```

4. **What about authorization?**
   ```
   Who can create projects in a team?
   Who can assign tasks?
   How would your API enforce this?
   ```

---

## Discussion Points:
- How do you validate projectId exists?
- What if assignee is not a team member?
- How do you handle duplicate task titles?
- Should task assignment be PUT /tasks/{id} or POST /tasks/{id}/assign?

---

**Time Limit**: 15 minutes
**Outcome**: RESTful API design that speaks business language 