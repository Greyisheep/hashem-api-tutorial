# Legacy Integration Challenge (15 minutes)

## The Problem: You must integrate with these legacy systems

Your TaskFlow API needs to integrate with existing enterprise systems. Your job is to design clean, modern API responses that hide the legacy ugliness from your API consumers.

---

## Legacy System 1: Weather Service

### Legacy Response:
```xml
<?xml version="1.0"?>
<WeatherResponse>
  <Status>OK</Status>
  <Data>
    <Location>New York</Location>
    <Temperature>72</Temperature>
    <TemperatureUnit>F</TemperatureUnit>
    <Humidity>65</Humidity>
    <Conditions>Partly Cloudy</Conditions>
    <WindSpeed>10</WindSpeed>
    <WindDirection>NW</WindDirection>
    <LastUpdated>2024-01-15T14:30:00Z</LastUpdated>
  </Data>
</WeatherResponse>
```

### Your Modern API Design:
**Endpoint**: `GET /projects/{id}/location/weather`

**Your Response:**
```json
{

}
```

---

## Legacy System 2: User Management API

### Legacy Response:
```json
{
  "result": "success",
  "data": {
    "user_info": {
      "user_id": "12345",
      "user_name": "john_doe",
      "user_email": "john@example.com",
      "user_status": "1",
      "user_created": "1642248600",
      "user_details": {
        "first_name": "John",
        "last_name": "Doe",
        "phone_number": "555-1234"
      }
    }
  }
}
```

### Your Modern API Design:
**Endpoint**: `GET /users/{id}`

**Your Response:**
```json
{

}
```

---

## Legacy System 3: Task Status Service

### Legacy Response:
```json
{
  "STATUS": "OK",
  "RESPONSE_DATA": {
    "TASK_RECORD": {
      "TASK_ID": "TSK_001_2024",
      "TASK_NAME": "IMPLEMENT_API_FEATURE",
      "TASK_DESC": "Build new REST endpoint for user management",
      "TASK_STATUS_CODE": "03",
      "TASK_STATUS_DESC": "IN_PROGRESS",
      "ASSIGNED_USER_ID": "USR_456",
      "CREATED_DATE": "20240115",
      "DUE_DATE": "20240130",
      "PRIORITY_LEVEL": "H"
    }
  },
  "ERROR_CODE": "0000",
  "ERROR_MESSAGE": ""
}
```

### Your Modern API Design:
**Endpoint**: `GET /tasks/{id}`

**Your Response:**
```json
{

}
```

---

## Requirements for Your Modern APIs:

### 1. Consistent Response Format
- Use camelCase for field names
- Consistent nesting structure
- Standard HTTP status codes

### 2. Modern Field Naming  
- Human-readable field names
- No abbreviations or codes
- Boolean values instead of "1"/"0"

### 3. Proper HTTP Status Codes
- 200 for successful data retrieval
- 404 when legacy system returns "not found"
- 503 when legacy system is down

### 4. Error Handling
- What happens when legacy system fails?
- How do you handle partial data?
- What about timeout scenarios?

---

## Your Error Handling Strategy:

### Legacy System Down
**Scenario**: Weather service returns 500 error

**Your API Response:**
```
Status Code: ____________

Body:
{

}
```

### Legacy System Returns Invalid Data
**Scenario**: User service returns malformed JSON

**Your API Response:**
```
Status Code: ____________

Body:
{

}
```

### Legacy System Timeout
**Scenario**: Task service takes 30 seconds to respond

**Your API Response:**
```
Status Code: ____________

Body:
{

}
```

---

## Expected Solutions:
*(Don't peek until you've tried!)*

<details>
<summary>Click to reveal suggested modern responses</summary>

### Weather Service Response:
```json
{
  "weather": {
    "location": "New York",
    "temperature": {
      "value": 72,
      "unit": "fahrenheit"
    },
    "humidity": 65,
    "conditions": "partly_cloudy",
    "wind": {
      "speed": 10,
      "direction": "northwest"
    },
    "lastUpdated": "2024-01-15T14:30:00Z"
  }
}
```

### User Management Response:
```json
{
  "user": {
    "id": "12345",
    "username": "john_doe",
    "email": "john@example.com",
    "status": "active",
    "createdAt": "2022-01-15T14:30:00Z",
    "profile": {
      "firstName": "John",
      "lastName": "Doe",
      "phoneNumber": "555-1234"
    }
  }
}
```

### Task Status Response:
```json
{
  "task": {
    "id": "TSK_001_2024",
    "title": "Implement API Feature",
    "description": "Build new REST endpoint for user management",
    "status": "in_progress",
    "assignedTo": "USR_456",
    "createdAt": "2024-01-15T00:00:00Z",
    "dueDate": "2024-01-30T00:00:00Z",
    "priority": "high"
  }
}
```

</details>

---

## Design Patterns:

### 1. Response Transformation
- Map legacy field names to modern ones
- Convert data types (string "1" → boolean true)
- Restructure nested objects for clarity

### 2. Error Masking
- Don't expose legacy error codes to consumers
- Translate technical errors to business errors
- Provide helpful error messages

### 3. Defensive Programming
- Handle missing fields gracefully
- Validate legacy responses before transforming
- Set reasonable timeouts

---

## Discussion Questions:

1. **How do you handle legacy field name changes?**
   ```
   Strategy:
   Example:
   ```

2. **What if the legacy system is unreliable?**
   ```
   Caching strategy:
   Fallback approach:
   ```

3. **Should you expose legacy system limitations?**
   ```
   Yes/No:
   Reasoning:
   ```

4. **How do you test integration with unstable legacy systems?**
   ```
   Testing approach:
   Mocking strategy:
   ```

---

## Real-World Integration War Stories:

**Share your experience:**
```
Worst legacy system you've worked with:

What made it difficult:

How you solved it:

What you'd do differently:
```

---

**Time Limit**: 15 minutes
**Outcome**: Clean API facades that hide legacy complexity 