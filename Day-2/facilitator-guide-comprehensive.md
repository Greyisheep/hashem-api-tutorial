# Day 2 Facilitator Guide - API Implementation & Architecture

## 🎯 Day 2 Objectives
By 4:00 PM, students should be able to:
- [ ] Review and improve takehome assignments from Day 1
- [ ] Conduct effective code reviews with domain focus
- [ ] Design and implement proper project structure and architecture
- [ ] Create database schemas using dbdiagram.io
- [ ] Implement Authentication & Authorization (OAuth 2.0, JWT, API keys)
- [ ] Apply production-ready patterns

**Key Deliverable**: Each student has a working API with proper architecture, database design, and authentication system.

---

## ⏰ Detailed Schedule & Timing

### 10:00-10:30 AM: Takehome Review & Production Setup (30 min)
**Activity**: "Show us your best API design from yesterday's takehome"
**Your Role**: Connect Day 1 concepts to today's implementation focus

#### Pre-flight Checklist:
- [ ] All students have .NET 8 SDK installed
- [ ] Docker Desktop running and accessible
- [ ] TaskFlow API repository cloned and ready
- [ ] Postman collections imported
- [ ] VS Code with C# extensions
- [ ] dbdiagram.io accounts ready
- [ ] Timer visible to group

#### Opening Script:
"Yesterday we designed APIs. Today we build them. Let's see how those domain models translate into working code!"

#### Takehome Review (20 min)
**Format**: Each student presents their best API design (3 min each)
- **Focus**: Domain language, REST principles, error handling
- **Your Role**: Ask probing questions, highlight good patterns
- **Key Questions**:
  - "How did you handle cross-domain operations?"
  - "What error scenarios did you consider?"
  - "How would this scale to 1000+ users?"

### 10:30-10:45 AM: Break (15 min)
**Your prep time**: Set up TaskFlow API demo environment

### 10:45-11:45 AM: Module 1 - Project Structure & Architecture (60 min)

#### Architecture Walkthrough (20 min)
**Materials**: `taskflow-api-dotnet/DDD-IMPLEMENTATION-SUMMARY.md`
- **Show**: Live running TaskFlow API
- **Focus**: "See how yesterday's domain model became this code"
- **Walk through**: Value objects, aggregates, repositories
- **Energy**: "This is what production DDD looks like"

#### Code Review Session (25 min)
**Activity**: Code walkthrough with student participation
- **Start**: Domain layer - "What business rules do you see?"
- **Move**: Application layer - "How does CQRS work here?"
- **End**: Infrastructure - "Why this abstraction?"

**Key Questions**:
- "Why is Email a value object, not a string?"
- "What happens if we try to change a task's status?"
- "How does the repository pattern help us?"

#### Project Structure Deep Dive (15 min)
**Show**: Complete project organization
- **Domain Layer**: Entities, value objects, domain events
- **Application Layer**: Commands, queries, services
- **Infrastructure Layer**: Repositories, external services
- **API Layer**: Controllers, middleware, configuration

### 11:45-12:00 PM: Break (15 min)
**Your prep time**: Prepare database design session

### 12:00-1:00 PM: Module 2 - Database Design with dbdiagram.io (60 min)

#### Database Design Principles (15 min)
**Discussion**: "How do you translate domain models to database schemas?"
- **Show**: Domain entities → Database tables
- **Focus**: Relationships, constraints, indexing
- **Key insight**: "Database design reflects domain boundaries"

#### dbdiagram.io Workshop (30 min)
**Activity**: Design TaskFlow database schema
- **Setup**: Everyone creates dbdiagram.io account
- **Task**: Design complete TaskFlow database
- **Include**: Users, Projects, Tasks, Teams, Permissions
- **Focus**: Relationships, foreign keys, indexes

**Your Role**: Walk around, help with relationships, validate designs

#### Schema Review & Optimization (15 min)
**Activity**: Review each other's schemas
- **Pair up**: Students review partner's design
- **Focus**: Performance, normalization, relationships
- **Key Questions**:
  - "How would this handle 10,000 tasks?"
  - "What indexes would you add?"
  - "How do you handle soft deletes?"

### 1:00-2:00 PM: Lunch (60 min)
**Your prep time**: Set up authentication demo environment

### 2:00-3:30 PM: Module 3 - Authentication & Authorization (90 min)

#### Authentication Fundamentals (20 min)
**Discussion**: "What are the different ways to authenticate API users?"
- **Show**: OAuth 2.0 flow diagrams
- **Compare**: JWT vs Session tokens
- **Key insight**: "Choose based on your use case"

#### OAuth 2.0 Implementation (30 min)
**Demo**: Complete OAuth 2.0 flow
- **Show**: Authorization code flow
- **Demonstrate**: Token exchange
- **Focus**: Security considerations

#### JWT Implementation (25 min)
**Code-Along**: Implement JWT authentication
- **Setup**: JWT token generation and validation
- **Show**: Token structure and claims
- **Focus**: Security best practices

#### API Keys & Authorization (15 min)
**Discussion**: "When do you use API keys vs OAuth?"
- **Show**: API key implementation
- **Demonstrate**: Rate limiting with API keys
- **Key insight**: "API keys for machine-to-machine, OAuth for user access"

### 3:30-3:45 PM: Break (15 min)
**Your prep time**: Prepare final implementation session

### 3:45-4:00 PM: Module 4 - Integration & Next Steps (15 min)

#### Implementation Integration (10 min)
**Activity**: Connect all pieces together
- **Show**: Complete system with auth
- **Demonstrate**: Database + API + Authentication
- **Focus**: End-to-end flow

#### Day 3 Preview (5 min)
**Preview**: Security deep dive and production deployment
- **Teaser**: "Tomorrow we lock this down and deploy it"

---

## 🎯 Facilitation Strategies

### For Implementation-Focused Day:
- **Hands-on priority**: More coding, less talking
- **Pair programming**: Students work together on exercises
- **Live debugging**: Use real errors as teaching moments
- **Celebrate wins**: "Look what you just built!"

### Energy Management:
- **Morning**: High energy with takehome review
- **Mid-morning**: Focus on architecture and structure
- **Afternoon**: Database design excitement
- **Late afternoon**: Authentication implementation
- **End**: Integration satisfaction

### Common Implementation Issues:
- **Docker problems**: "Let's debug this together"
- **Code errors**: "What's the error telling us?"
- **Database confusion**: "Let's draw this out"

---

## 🧪 Hands-on Exercises

### Exercise 1: Database Design (30 min)
**Objective**: Design complete TaskFlow database schema

**Success Criteria**:
- [ ] All domain entities represented
- [ ] Proper relationships defined
- [ ] Appropriate indexes identified
- [ ] Performance considerations noted

**Your Role**: Help with relationships, validate designs

### Exercise 2: Authentication Implementation (45 min)
**Objective**: Implement JWT authentication in TaskFlow API

**Success Criteria**:
- [ ] JWT token generation works
- [ ] Token validation implemented
- [ ] Protected endpoints secured
- [ ] Error handling in place

**Your Role**: Help debug, guide security thinking

### Exercise 3: Code Review Practice (20 min)
**Objective**: Practice effective code review

**Success Criteria**:
- [ ] Domain-focused feedback given
- [ ] Security issues identified
- [ ] Performance concerns raised
- [ ] Positive feedback balanced

**Your Role**: Model good review practices

---

## 📊 Assessment & Validation

### During Implementation:
**Look for**:
- [ ] Understanding of DDD patterns
- [ ] Proper database design
- [ ] Security implementation
- [ ] Code review skills

**Red flags**:
- [ ] Copy-paste without understanding
- [ ] Ignoring security considerations
- [ ] Poor database relationships
- [ ] No code review participation

### End of Day Checklist:
**Each student should have**:
- [ ] Reviewed takehome assignments
- [ ] Understanding of project architecture
- [ ] Database schema designed
- [ ] Authentication implemented
- [ ] Code review experience
- [ ] Clear next steps for Day 3

---

## 🛠️ Technical Setup & Troubleshooting

### Common Issues:
**Docker problems**: `docker-compose down && docker-compose up --build`
**Database issues**: Reset with `docker-compose down -v`
**API issues**: Check logs with `docker-compose logs taskflow-api`
**dbdiagram.io issues**: Use backup browser or mobile app

### Backup Plans:
**Demo failures**: Use pre-recorded videos
**Environment issues**: Use cloud-based alternatives
**Time pressure**: Focus on core authentication implementation
**Technical difficulties**: Pair students for support

---

## 📊 Success Metrics

### Immediate (End of Day 2):
- [ ] Students can conduct effective code reviews
- [ ] Can design proper database schemas
- [ ] Understand authentication patterns
- [ ] Can implement JWT authentication
- [ ] Have working project architecture

### Forward-looking (Setup for Day 3):
- [ ] Students confident with implementation
- [ ] Ready for security deep dive
- [ ] Understand production deployment
- [ ] Excited about advanced topics

---

## 🎪 Instructor Energy & Tone

### Maintain Implementation Focus:
- **"Let's build this!"**: Hands-on energy
- **"What's the error telling us?"**: Debugging mindset
- **"Look what you just created!"**: Celebration
- **"How would this work in production?"**: Real-world thinking

### Avoid:
- **Theory overload**: Keep concepts tied to implementation
- **Perfect code**: Focus on working solutions
- **Solo coding**: Encourage collaboration
- **Rushing**: Better working code than rushed code

### Recovery Strategies:
**If behind**: Skip optional demos, focus on core authentication
**If ahead**: Deeper implementation challenges
**If stuck**: Pair programming, group debugging
**If confused**: Stop, simplify, rebuild

---

**Remember**: Today is about building, not just learning. Students should leave with working code and confidence in their implementation skills.

**Day 2 Success**: Students have built, reviewed, and secured real APIs using production patterns. 