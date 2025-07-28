# Day 1: Foundation & Design - "Build the Blueprint"

🎯 **Focus**: Domain-driven API architecture with production mindset

## Learning Objectives

By the end of Day 1, students will be able to:
- [ ] Classify APIs by type and use case (REST, GraphQL, gRPC, SOAP, WebSocket)
- [ ] Map business domains to API structure  
- [ ] Design REST endpoints using domain language
- [ ] Choose appropriate HTTP status codes
- [ ] Explain when to use different API protocols (REST vs GraphQL vs gRPC)
- [ ] Design clean facades for legacy systems
- [ ] Understand API lifecycle and best practices

## Schedule Overview

| Time | Module | Activity | Duration |
|------|--------|----------|----------|
| 10:00-10:15 AM | Intros & Reality Check | Worst API experiences | 15 min |
| 10:15-10:45 AM | **Module 1** - API Experience & Types | API Treasure Hunt + Classification | 30 min |
| 10:45-11:00 AM | *Break* | | 15 min |
| 11:00-12:00 PM | **Module 2A** - Domain-Driven Architecture | TaskFlow Demo + Domain Modeling | 60 min |
| 12:00-1:00 PM | *Lunch* | | 60 min |
| 1:00-1:45 PM | **Module 2B** - API Design + Documentation | Domain-to-REST Mapping | 45 min |
| 1:45-2:30 PM | **Module 2C** - Error Handling & Edge Cases | Status Code Scenarios | 45 min |
| 2:30-2:45 PM | *Break* | | 15 min |
| 2:45-3:30 PM | **Module 2D** - gRPC vs REST Deep Dive | Protocol Comparison | 45 min |
| 3:30-3:45 PM | **Module 2E** - Integration Reality Check | Legacy API Challenge | 15 min |
| 3:45-4:00 PM | **Module 2F** - Repository Collaboration | GitHub Setup + Take-Home | 15 min |

## Key Activities

### 🎯 API Treasure Hunt (10 min)
Explore 6 APIs to identify good and bad design patterns:
- **Our Local TaskFlow API** (baseline)
- JSONPlaceholder (educational)
- GitHub API (enterprise)  
- OpenWeather (commercial)
- REST Countries (simple)
- HTTPBin (testing tool)
- Legacy SOAP service (problematic)

### 🎯 Domain Modeling Lab (20 min)
Map TaskFlow business domains:
- Team Management
- Project Planning  
- Task Execution
- User Identity
- Communication

### 🎯 Domain-to-REST Mapping (15 min)
Transform business domains into RESTful endpoints:
- Resource-based URLs
- HTTP verbs for actions
- Nested relationships
- Query parameters for filtering

### 🎯 Status Code Scenarios (15 min)
Practice choosing correct HTTP status codes for 15 real-world scenarios

### 🎯 Legacy API Challenge (15 min)
Design clean, modern API facades for ugly legacy responses

## Resources

### Course Materials
- [Complete Implementation Guide](./Day%201%20Complete%20Implementation%20Guide%20-%20API%20Foundation%20&%20Design.pdf)
- [Resource Links & Setup Guide](./Day%201%20Resource%20Links%20&%20Setup%20Guide.pdf)  
- [Slide Deck](./Day%201%20Slide%20Deck%20-%20API%20Foundation%20&%20Design.pdf)

### Directory Structure
```
Day-1/
├── activities/       # Hands-on exercises
├── demos/           # Instructor demonstrations
├── resources/       # Templates and worksheets  
├── exercises/       # Student practice work
├── slides/         # Presentation materials
└── workshops/      # Interactive workshops
```

### Public APIs for Activities
- **JSONPlaceholder**: `https://jsonplaceholder.typicode.com`
- **GitHub API**: `https://api.github.com`
- **REST Countries**: `https://restcountries.com/v3.1`
- **OpenWeather**: `https://api.openweathermap.org/data/2.5`
- **HTTPBin**: `https://httpbin.org`

### Required Tools
- [ ] Docker Desktop installed and running
- [ ] Postman Desktop (or web version)
- [ ] VS Code with REST Client extension
- [ ] Web browser with bookmarks  
- [ ] Excalidraw access for domain modeling
- [ ] GitHub account (for contributions)

## Collaborative Learning Approach

### 🚀 Working API Foundation
- **Real Implementation**: Students work with an actual running FastAPI application
- **Immediate Feedback**: See results instantly with Docker setup
- **Hands-On Experience**: Modify and extend real code, not theoretical examples

### 👥 Repository Contributions
- **Fork & Contribute**: Each student forks the main repository
- **Domain Implementation**: Add new domains (Users, Comments, Notifications, etc.)
- **Code Reviews**: Review and merge peer contributions
- **Real Collaboration**: Experience actual team development workflow

### 📚 Progressive Learning
- **Day 1**: Basic CRUD operations and API design
- **Day 2**: Build upon student contributions with advanced features
- **Day 3**: Add security to the growing codebase
- **Day 4**: Deploy the collaborative project
- **Day 5**: Advanced features and business impact

## Deliverables

### Day 1 Outcome
Complete TaskFlow API design document including:
- [ ] Domain model with clear boundaries
- [ ] REST endpoint specifications
- [ ] Error response patterns
- [ ] API type decision framework
- [ ] Forked taskflow-api repository
- [ ] Submitted PR with new domain implementation
- [ ] Reviewed peer contributions
- [ ] Legacy integration strategy

### Take-Home Assignment
Set up FastAPI project with domain structure ready for Day 2 implementation.

## Success Criteria

Students successfully complete Day 1 when they can:
1. **Design APIs** that speak business language, not database language
2. **Choose the right protocol** (REST/gRPC/GraphQL) for each use case  
3. **Handle errors professionally** with appropriate status codes and responses
4. **Integrate legacy systems** through clean, modern facades
5. **Think domain-first** when designing API architecture

---

**Next**: [Day 2: Implementation - "Code Like Production"](../Day-2/) 