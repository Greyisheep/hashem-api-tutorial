# API Treasure Hunt - 10 Minutes

## Your Mission: Explore these 6 APIs and document findings

### API 0: Our Local TaskFlow API (Baseline)
- **Base URL**: `http://localhost:8001`
- **Try**: `GET /health`, `GET /tasks`, `GET /user-stories`, `GET /response-patterns/envelope`
- **Find**: How does our API compare to production APIs? Notice the envelope pattern and user stories integration.

**Your Notes:**
```
Response quality: 
Documentation: 
Developer experience: 
What would you improve?:
```

### API 1: JSONPlaceholder (Good Example)
- **Base URL**: `https://jsonplaceholder.typicode.com`
- **Try**: `GET /posts/1`
- **Find**: What makes this response developer-friendly?

**Your Notes:**
```
Response quality: 
Authentication: 
Documentation: 
Would you integrate? (Y/N): 
Why?:
```

### API 2: GitHub API (Excellent Example)  
- **Base URL**: `https://api.github.com`
- **Try**: `GET /users/octocat`
- **Find**: How does GitHub handle API versioning?

**Your Notes:**
```
Response quality: 
Authentication: 
Documentation: 
Would you integrate? (Y/N): 
Why?:
```

### API 3: OpenWeather (Commercial Example)
- **Base URL**: `https://api.openweathermap.org/data/2.5`
- **Try**: `GET /weather?q=London&appid=demo`
- **Find**: How do they handle authentication?

**Your Notes:**
```
Response quality: 
Authentication: 
Documentation: 
Would you integrate? (Y/N): 
Why?:
```

### API 4: REST Countries (Simple Example)
- **Base URL**: `https://restcountries.com/v3.1`
- **Try**: `GET /name/france`
- **Find**: What's good/bad about the response structure?

**Your Notes:**
```
Response quality: 
Authentication: 
Documentation: 
Would you integrate? (Y/N): 
Why?:
```

### API 5: HTTPBin (Testing Tool)
- **Base URL**: `https://httpbin.org`
- **Try**: `GET /get`, `POST /post`, `GET /status/404`
- **Find**: How useful is this for API testing?

**Your Notes:**
```
Response quality: 
Testing capabilities: 
Documentation: 
Would you use for testing? (Y/N): 
Why?:
```

### API 6: Legacy SOAP Example (Problem Example)
- **URL**: `http://webservices.oorsprong.org/websamples.countryinfo/CountryInfoService.wso`
- **Find**: What makes this difficult to work with?

**Your Notes:**
```
Response quality: 
Authentication: 
Documentation: 
Would you integrate? (Y/N): 
Why?:
```

## Questions to Answer:
1. **Which API would you want to integrate with? Why?**
   ```
   API: 
   Reason:
   ```

2. **Which API would you avoid? Why?**
   ```
   API: 
   Reason:
   ```

3. **What patterns do you notice in the good APIs?**
   ```
   Pattern 1:
   Pattern 2:
   Pattern 3:
   ```

## Bonus Challenge:
Try making the same request to each API using:
- [ ] Postman
- [ ] VS Code REST Client
- [ ] Browser (for GET requests)

**Winner**: Which tool felt most productive for API exploration?

---

**Time Limit**: 10 minutes
**Share**: Be ready to present your top finding to the group! 