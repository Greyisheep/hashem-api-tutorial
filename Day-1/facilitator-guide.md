# Day 1 Facilitator Guide - API Foundation & Design

## 🎯 Day 1 Objectives
By 4:00 PM, students should be able to:
- [ ] Classify APIs by type and use case
- [ ] Map business domains to API structure
- [ ] Design REST endpoints using domain language
- [ ] Choose appropriate HTTP status codes
- [ ] Explain when to use gRPC vs REST
- [ ] Design clean facades for legacy systems

**Key Deliverable**: Each student has a complete TaskFlow API design document ready for Day 2 implementation.

---

## ⏰ Detailed Schedule & Timing

### 10:00-10:15 AM: Intros & Reality Check (15 min)
**Activity**: Everyone shares their worst API integration experience (2 min each)
**Your Role**: Set energy, verify environment, establish "production mindset"

#### Pre-flight Checklist:
- [ ] All students have Docker Desktop installed and running
- [ ] All students can access the taskflow-api repository
- [ ] Postman Desktop working (or web version as backup)
- [ ] VS Code with REST Client extension installed
- [ ] Can access Excalidraw/drawing tool
- [ ] Timer visible to group
- [ ] Sticky notes and markers ready

#### Opening Script:
"We're not here to learn theory - we're here to build production-ready APIs. Let's start with reality: what's the worst API integration you've ever had to deal with? 2 minutes each, go!"

### 10:15-10:45 AM: Module 1 - API Experience & Types (30 min)

#### API Treasure Hunt (10 min)
**Materials**: [`activities/api-treasure-hunt.md`](./activities/api-treasure-hunt.md)
- **Timer**: Visible 10-minute countdown
- **Energy**: High, encouraging exploration
- **Walk around**: Ask "What are you finding?" "Any surprises?"

#### Real-World Discussion (10 min)
**Question**: "What makes an API production-ready?"
- Expected answers: Documentation, consistent responses, error handling
- Guide toward: Reliability, developer experience, business language

#### API Classification Challenge (10 min)  
**Materials**: [`activities/api-classification-challenge.md`](./activities/api-classification-challenge.md)
- **Pair up**: 2 pairs of 2 students
- **Competition**: "Which pair gets more correct?"
- **Debrief**: Focus on constraints driving decisions
- **Key Learning**: REST vs GraphQL vs gRPC decision framework

### 10:45-11:00 AM: Break (15 min)
**Your prep time**: Set up Excalidraw board, prepare demo environment

### 11:00-12:00 PM: Module 2A - Domain-Driven TaskFlow Architecture (60 min)

#### Your Demo First (12 min)
**Materials**: [`demos/taskflow-api-demo-script.md`](./demos/taskflow-api-demo-script.md)
- **Key message**: APIs should speak business language and use consistent response patterns
- **Show**: Live running API with Docker, envelope pattern, user stories, business-focused URLs
- **Energy**: Conversational, pause for questions
- **Setup**: Run `docker-compose up --build` in taskflow-api directory

#### DDD Fundamentals (15 min)
**Discussion prompts**:
- "Looking at TaskFlow, what business capabilities do you see?"
- "Which of these could exist independently?"
- "Where do you see the most complexity?"
- "How do user stories help us understand domain boundaries?"

**Your role**: Guide, don't lecture. Let them discover domain boundaries.

#### Domain Modeling Lab (20 min)
**Materials**: [`workshops/domain-modeling-template.md`](./workshops/domain-modeling-template.md)
- **Physical setup**: Sticky notes (Yellow=Entities, Blue=Services, Pink=Events)
- **Digital option**: Shared Excalidraw board
- **Walk around**: Ask probing questions from guide
- **Energy**: Active facilitation, not passive watching

#### Microservices Reality (15 min)
**Question**: "How do domain boundaries influence deployment?"
- Connect domains to potential microservices
- Discuss data consistency across boundaries
- Preview complexity of distributed systems

### 12:00-1:00 PM: Lunch (60 min)
**Your prep time**: Review domain modeling results, prepare for afternoon

### 1:00-1:45 PM: Module 2B - API Design + Documentation (45 min)

#### Domain-to-REST Mapping (15 min)
**Materials**: [`exercises/domain-to-rest-mapping.md`](./exercises/domain-to-rest-mapping.md)
- **Build on**: Morning's domain model
- **Focus**: Business language in URLs and user stories
- **Common mistake**: Technical database thinking

#### Cross-Domain Operations (10 min)
**Question**: "How do you handle operations that span domains?"
- Example: "Get all tasks assigned to user X across all projects"
- Discuss trade-offs: convenience vs domain purity

#### OpenAPI Contract Design (10 min)
**Activity**: Write OpenAPI spec for task creation
- **Focus**: Business requirements driving API design, including user stories
- **Validate**: Does spec reflect domain language and response patterns?

#### Documentation Standards (10 min)
**Question**: "How do you keep docs synchronized with code?"
- Discuss API-first design
- Show example of good vs bad documentation

### 1:45-2:30 PM: Module 2C - Error Handling & Edge Cases (45 min)

#### Status Code Scenarios (15 min)
**Materials**: [`exercises/status-code-scenarios.md`](./exercises/status-code-scenarios.md)
- **Format**: Individual work first, then pair discussion
- **Focus**: Business implications of status codes
- **Common debates**: 400 vs 422, when to use 409

#### Production Error Handling (15 min)
**Question**: "How do you handle cascading failures?"
- Discuss circuit breakers, timeouts
- Real-world war stories
- **Key point**: Envelope pattern helps with consistent error handling

#### Error Response Design (15 min)
**Activity**: Design professional error responses
- **Compare**: Technical vs business-friendly errors
- **Focus**: Actionable error messages and envelope pattern benefits

### 2:30-2:45 PM: Break (15 min)
**Your prep time**: Prepare gRPC demo environment

### 2:45-3:30 PM: Module 2D - gRPC vs REST Deep Dive (45 min)

#### Your Demo (15 min)
**Materials**: [`demos/grpc-demo-materials.md`](./demos/grpc-demo-materials.md)
- **Show**: Live performance comparison
- **Highlight**: Streaming capabilities
- **Energy**: Technical excitement, but accessible

#### Internal vs External APIs (15 min)
**Question**: "When do you choose gRPC over REST?"
- **Framework**: Constraints, not preferences
- **Real examples**: Public API (REST) vs internal services (gRPC)

#### Protocol Buffer Design (15 min)
**Activity**: Design .proto for TaskFlow notifications
- **Focus**: Type safety and contracts
- **Compare**: To JSON schema design

### 3:30-3:45 PM: Module 2E - Integration Reality Check (15 min)

#### Legacy API Challenge (15 min)
**Materials**: [`exercises/legacy-integration-challenge.md`](./exercises/legacy-integration-challenge.md)
- **Reality**: 80% of API work is integration
- **Focus**: Clean facades hiding ugly legacy systems
- **Energy**: Fun and relatable - everyone has legacy pain

### 3:45-4:00 PM: Module 2F - Repository Collaboration & Take-Home (15 min)

#### GitHub Setup & Contribution (10 min)
**Activity**: Fork and contribute to taskflow-api repository
- **Setup**: Each student forks the main repository
- **Task**: Add a new endpoint to the API (e.g., `/users` endpoint)
- **Process**: Create feature branch, implement, test, submit PR
- **Learning**: Real-world collaboration workflow

#### Take-Home Assignment (5 min)
**Materials**: [`exercises/take-home-assignment.md`](./exercises/take-home-assignment.md)
- **Individual**: Extend the API with new domain (e.g., notifications, comments)
- **Collaborative**: Review and merge each other's PRs
- **Documentation**: Update API documentation
- **Due**: Before Day 2 starts

---

## 🎯 Facilitation Strategies

### For 4-Student Groups:
- **Pair activities**: 2 pairs of 2 for focused discussion
- **Round-robin sharing**: Everyone presents in each activity
- **Collaborative boards**: All 4 work on same Excalidraw
- **Friendly competition**: Pairs compete on design challenges
- **Direct questions**: Call on individuals by name

### Energy Management:
- **High energy start**: Jump right into API treasure hunt
- **Post-lunch dip**: Make 1:00 PM highly interactive (REST mapping)
- **End strong**: Legacy challenge is fun and relatable
- **Use timers**: Visible countdown creates urgency
- **Movement**: Don't let them sit for more than 20 minutes

### Common Pitfalls to Address:
- **Over-engineering**: "Keep it simple, add complexity when needed"
- **Technical jargon**: "How would you explain this to a product manager?"
- **Perfect solutions**: "What would break first at scale?"
- **Database thinking**: "APIs should speak business language, not database language"

---

## 🗣️ Discussion Facilitation

### Question Types:
**Open questions**: "What do you think about..."
**Probing questions**: "Why did you choose..."
**Connecting questions**: "How does this relate to..."
**Reality check questions**: "What would happen in production if..."

### When Students Get Stuck:
1. **Reframe the question**: Business perspective vs technical
2. **Give examples**: "Imagine you're explaining to your manager..."
3. **Use analogies**: Physical world comparisons
4. **Pair them up**: Let students teach each other

### Managing Debates:
- **Acknowledge both sides**: "That's a valid perspective..."
- **Find the constraint**: "What would drive you toward one choice?"
- **Real-world context**: "In production, you'd need to consider..."
- **Move forward**: Don't let perfect be the enemy of good

---

## 📋 Assessment & Validation

### During Activities:
**Look for**:
- [ ] Business language in URL designs
- [ ] Correct status code reasoning
- [ ] Domain boundaries that make sense
- [ ] Understanding of protocol trade-offs

**Red flags**:
- [ ] Technical/database language in APIs
- [ ] Generic endpoints like `/getData`
- [ ] Confusion about status code semantics
- [ ] Thinking one protocol is always better

### End of Day Checklist:
**Each student should have**:
- [ ] Clear TaskFlow domain model
- [ ] REST endpoint specifications
- [ ] Error response patterns
- [ ] API type decision framework
- [ ] Legacy integration strategy
- [ ] Forked taskflow-api repository
- [ ] Submitted at least one PR with new endpoint
- [ ] Reviewed at least one peer's PR

---

## 🛠️ Technical Setup & Troubleshooting

### Common Issues:
**Postman problems**: Use web version as backup
**Excalidraw access**: Google Drawings as alternative
**API endpoints down**: Have screenshots as backup
**VS Code issues**: Browser dev tools work for GET requests

### Backup Plans:
**No internet**: Printed worksheets for all activities
**Tool failures**: Physical sticky notes and whiteboards
**Demo failures**: Pre-recorded screenshots and videos
**Time pressure**: Skip OpenAPI contract activity if needed

---

## 📊 Success Metrics

### Immediate (End of Day 1):
- [ ] Students can explain DDD principles in their own words
- [ ] Can design RESTful endpoints using business language
- [ ] Understand when to choose different API protocols
- [ ] Can handle error scenarios appropriately

### Forward-looking (Setup for Day 2):
- [ ] Students excited about implementation
- [ ] Clear on domain boundaries for TaskFlow
- [ ] Ready to translate design into FastAPI code
- [ ] Understand production mindset

---

## 🎪 Instructor Energy & Tone

### Maintain High Energy:
- **Enthusiasm for APIs**: "This is the fun part!"
- **Real-world relevance**: "You'll use this next week"
- **Student success focus**: "You're getting it!"
- **Curiosity encouragement**: "Great question!"

### Avoid:
- **Lecturing**: Keep talking segments under 10 minutes
- **Perfectionism**: "Good enough to move forward"
- **Technical showing off**: Stay at their level
- **Rushing**: Better to skip content than rush through it

### Recovery Strategies:
**If behind**: Skip optional discussions, prioritize hands-on
**If ahead**: Deeper dives into student questions
**If energy drops**: Stand up, move around, change format
**If confused**: Stop, reframe, use different approach

---

**Remember**: You're guiding discovery, not delivering information. Let them figure out the domain boundaries. Let them struggle with status codes. Your job is to facilitate their learning, not to tell them the answers.

**Day 1 Success**: Students leave excited to build TaskFlow APIs and confident in their design decisions. 