# Code Review Workshop - Domain-Driven Review

## 🎯 Workshop Objectives
- Practice effective code review techniques
- Focus on domain-driven design principles
- Identify security and performance issues
- Build collaborative review skills

## ⏰ Workshop Timing: 25 minutes

---

## 🚀 Code Review Fundamentals (5 minutes)

### What Makes a Good Code Review?
**Question**: "What should you look for in a code review?"

#### Domain-Driven Review Focus:
1. **Business Logic**: Does the code reflect domain concepts?
2. **Domain Language**: Are names and methods business-focused?
3. **Aggregate Boundaries**: Are domain boundaries respected?
4. **Value Objects**: Are business rules encapsulated?
5. **Domain Events**: Are important business events captured?

#### Technical Review Focus:
1. **Security**: Authentication, authorization, input validation
2. **Performance**: Database queries, caching, algorithms
3. **Maintainability**: Code structure, naming, documentation
4. **Testing**: Test coverage, test quality
5. **Error Handling**: Proper exception handling

---

## 🎯 Hands-on Review Exercise (15 minutes)

### Review Task: Task Creation Endpoint

#### Code to Review:
```csharp
// TaskFlow.API/Controllers/TasksController.cs
[HttpPost]
public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
{
    try
    {
        // Validate input
        if (string.IsNullOrEmpty(request.Title))
            return BadRequest("Title is required");
            
        if (string.IsNullOrEmpty(request.Description))
            return BadRequest("Description is required");
            
        // Create task
        var task = new Task
        {
            Id = Guid.NewGuid().ToString(),
            Title = request.Title,
            Description = request.Description,
            Status = "pending",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        
        // Save to database
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();
        
        return Ok(task);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating task");
        return StatusCode(500, "Internal server error");
    }
}
```

### Review Checklist (5 minutes)
**Individual Review**: Each student reviews the code

#### Domain-Driven Questions:
- [ ] Does this respect domain boundaries?
- [ ] Are business rules properly enforced?
- [ ] Is domain language used consistently?
- [ ] Are value objects used appropriately?
- [ ] Are domain events captured?

#### Technical Questions:
- [ ] Is input validation sufficient?
- [ ] Is error handling appropriate?
- [ ] Are there security concerns?
- [ ] Is the code maintainable?
- [ ] Are there performance issues?

### Group Discussion (10 minutes)
**Facilitated Discussion**: Share findings and suggestions

#### Key Issues to Identify:
1. **Domain Issues**:
   - No value objects for Title, Description
   - No domain validation
   - No domain events
   - String-based status instead of enum

2. **Security Issues**:
   - No authentication/authorization
   - No input sanitization
   - Exposed internal errors

3. **Technical Issues**:
   - Manual ID generation
   - No transaction handling
   - Poor error messages
   - No logging structure

---

## 🔍 Review Practice Exercise (5 minutes)

### Pair Review Activity
**Task**: Review partner's TaskFlow implementation

#### Review Template:
```markdown
## Code Review Feedback

### What I Liked:
- [Positive feedback]

### Domain-Driven Issues:
- [Domain-related concerns]

### Security Issues:
- [Security concerns]

### Performance Issues:
- [Performance concerns]

### Suggestions:
- [Specific improvements]

### Questions:
- [Clarification questions]
```

#### Review Guidelines:
- **Be constructive**: Focus on improvement, not criticism
- **Be specific**: Point to exact lines or patterns
- **Be helpful**: Suggest solutions, not just problems
- **Be respectful**: Remember the person wrote this code

---

## 🎯 Success Criteria

### Excellent Review (All of these):
- [ ] Identifies domain-driven issues
- [ ] Finds security vulnerabilities
- [ ] Suggests specific improvements
- [ ] Provides constructive feedback
- [ ] Asks clarifying questions

### Good Review (Most of these):
- [ ] Identifies some technical issues
- [ ] Provides some suggestions
- [ ] Gives constructive feedback
- [ ] Participates in discussion
- [ ] Shows understanding of concepts

### Needs Improvement (Few of these):
- [ ] Misses major issues
- [ ] Provides only negative feedback
- [ ] No specific suggestions
- [ ] Doesn't participate
- [ ] Shows poor understanding

---

## 📝 Facilitator Notes

### Common Review Patterns:
- **Domain Language**: "Should this be a value object?"
- **Business Rules**: "What business rule is being enforced here?"
- **Security**: "How do we authenticate this request?"
- **Performance**: "Will this scale with more data?"
- **Maintainability**: "How easy is this to understand and modify?"

### Questions to Ask:
- "What domain concept does this represent?"
- "How would this handle edge cases?"
- "What happens if this fails?"
- "How would you test this?"
- "What security concerns do you see?"

### Energy Management:
- **Encourage participation** - "What do you think about this?"
- **Validate good observations** - "Excellent catch!"
- **Guide discussion** - "Let's explore that further"
- **Build confidence** - "You're thinking like a senior developer"

### Common Issues to Watch For:
- **Overly critical**: Focus on improvement, not criticism
- **Too technical**: Remember domain-driven focus
- **Not specific**: Encourage specific line references
- **No suggestions**: Ask for improvement ideas

---

## 🚀 Next Steps

### Take-Home Practice:
**Task**: Review your own TaskFlow implementation
- Apply the review checklist to your code
- Identify 3-5 improvement areas
- Create a plan to address them
- Document your findings

### Day 3 Preview:
"Tomorrow we'll apply these review skills to security-focused code review!" 