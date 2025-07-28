# HTTP Status Code Challenge (15 minutes)

## Instructions: Choose the correct status code + explain why

### Pre-Exercise: Test Our API
```bash
# Start the API
cd taskflow-api
docker-compose up --build

# Test error scenarios with envelope pattern
curl http://localhost:8001/error/400
curl http://localhost:8001/error/404
curl http://localhost:8001/error/500
curl http://localhost:8001/tasks/nonexistent
```

**Notice**: Our API uses the envelope pattern for consistent error responses with standardized structure.

### Scenario 1: Create Task
**Request:** `POST /projects/123/tasks`
**Situation:** Project 123 doesn't exist
**Your Answer:** _____ 
**Options:** 400, 404, 422, 500
**Explanation:** ________________________________

### Scenario 2: Update Task
**Request:** `PUT /tasks/456`
**Situation:** Task exists but user doesn't have permission
**Your Answer:** _____
**Options:** 401, 403, 404, 409
**Explanation:** ________________________________

### Scenario 3: Assign Task  
**Request:** `POST /tasks/789/assign {"userId": "abc"}`
**Situation:** User exists but is not team member
**Your Answer:** _____
**Options:** 400, 403, 422, 409
**Explanation:** ________________________________

### Scenario 4: Delete Project
**Request:** `DELETE /projects/321`
**Situation:** Project has active tasks
**Your Answer:** _____
**Options:** 400, 409, 422, 500
**Explanation:** ________________________________

### Scenario 5: Get Team Projects
**Request:** `GET /teams/999/projects`
**Situation:** Database is temporarily down
**Your Answer:** _____
**Options:** 500, 502, 503, 504
**Explanation:** ________________________________

### Scenario 6: Rate Limit Exceeded
**Request:** `GET /teams`
**Situation:** User has made 100 requests in 1 minute (limit is 60)
**Your Answer:** _____
**Options:** 400, 403, 429, 503
**Explanation:** ________________________________

### Scenario 7: Invalid JSON Request
**Request:** `POST /teams`
**Body:** `{"name": "Engineering"` *(missing closing brace)*
**Your Answer:** _____
**Options:** 400, 422, 500
**Explanation:** ________________________________

### Scenario 8: Duplicate Team Name
**Request:** `POST /teams {"name": "Engineering"}`
**Situation:** Team named "Engineering" already exists
**Your Answer:** _____
**Options:** 400, 409, 422
**Explanation:** ________________________________

### Scenario 9: File Upload Too Large
**Request:** `POST /tasks/123/attachments`
**Situation:** File is 50MB, limit is 10MB
**Your Answer:** _____
**Options:** 400, 413, 422, 507
**Explanation:** ________________________________

### Scenario 10: Authentication Token Expired
**Request:** `GET /teams`
**Situation:** JWT token is valid but expired 1 hour ago
**Your Answer:** _____
**Options:** 400, 401, 403, 419
**Explanation:** ________________________________

### Scenario 11: Create Task Success
**Request:** `POST /projects/123/tasks`
**Situation:** Task created successfully
**Your Answer:** _____
**Options:** 200, 201, 202, 204
**Explanation:** ________________________________

### Scenario 12: Update Task (No Changes)
**Request:** `PUT /tasks/456`
**Body:** *(same data as current task)*
**Situation:** Task updated but nothing actually changed
**Your Answer:** _____
**Options:** 200, 204, 304
**Explanation:** ________________________________

### Scenario 13: Get Non-Existent Task
**Request:** `GET /tasks/999`
**Situation:** Task ID 999 doesn't exist
**Your Answer:** _____
**Options:** 400, 404, 410
**Explanation:** ________________________________

### Scenario 14: Validation Failure
**Request:** `POST /tasks`
**Body:** `{"title": "", "priority": "super-urgent"}`
**Situation:** Title is required, priority must be low/medium/high
**Your Answer:** _____
**Options:** 400, 422, 500
**Explanation:** ________________________________

### Scenario 15: Server Dependency Failure
**Request:** `POST /tasks/123/notifications`
**Situation:** Email service is down but task was created
**Your Answer:** _____
**Options:** 200, 201, 207, 500
**Explanation:** ________________________________

---

## Status Code Reference:

### Success (2xx):
- **200 OK** - Standard success response
- **201 Created** - Resource created successfully
- **202 Accepted** - Request accepted for processing
- **204 No Content** - Success but no response body
- **207 Multi-Status** - Multiple operations with different results

### Client Error (4xx):
- **400 Bad Request** - Malformed request syntax
- **401 Unauthorized** - Authentication required or failed
- **403 Forbidden** - Authenticated but insufficient permissions
- **404 Not Found** - Resource doesn't exist
- **409 Conflict** - Request conflicts with current state
- **410 Gone** - Resource used to exist but is permanently deleted
- **413 Payload Too Large** - Request body too large
- **422 Unprocessable Entity** - Syntactically correct but semantically wrong
- **429 Too Many Requests** - Rate limit exceeded

### Server Error (5xx):
- **500 Internal Server Error** - Generic server error
- **502 Bad Gateway** - Invalid response from upstream server
- **503 Service Unavailable** - Server temporarily unavailable
- **504 Gateway Timeout** - Upstream server timeout

---

## Discussion Questions:

1. **When do you use 422 vs 400?**
   ```
   400: 
   422:
   ```

2. **What's the difference between 401 and 403?**
   ```
   401: 
   403:
   ```

3. **How do you handle partial failures?**
   ```
   Example scenario:
   Status code choice:
   Response body approach:
   ```

4. **Should business rule violations be 400 or 422?**
   ```
   Your opinion:
   Example:
   ```

---

## Real-World Scenarios:

Design status codes for these business situations:

### A. **TeamFlow Specific**
- User tries to assign task to themselves but they're not a team member
- Manager tries to delete a team with active projects  
- User uploads task attachment larger than 10MB limit

### B. **Your Experience**
Think of a challenging status code decision from your work:
```
Scenario:
Status code chosen:
Alternative options considered:
Why you chose it:
```

---

## Answer Key:
*(Don't peek until you've attempted all scenarios!)*

<details>
<summary>Click to reveal answers</summary>

1. **404** - Project resource doesn't exist
2. **403** - User authenticated but lacks permission
3. **422** - Valid request but business rule violation
4. **409** - Conflict with current state (has active tasks)
5. **503** - Service temporarily unavailable
6. **429** - Rate limit exceeded
7. **400** - Malformed JSON syntax
8. **409** - Conflict with existing resource
9. **413** - Payload too large
10. **401** - Invalid/expired authentication
11. **201** - Resource created successfully
12. **204** - Success but no content changed
13. **404** - Resource not found
14. **422** - Validation failed (semantic error)
15. **201** - Main operation succeeded (task created)

</details>

---

**Time Limit**: 15 minutes
**Challenge**: Can you explain each answer in business terms? 