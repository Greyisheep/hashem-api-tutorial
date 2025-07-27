# Domain Modeling Exercise - Hands-On Workshop

## Materials Needed:
- Sticky notes (Yellow=Entities, Blue=Services, Pink=Events)
- Large paper/whiteboard
- Markers
- **OR** Excalidraw shared board (digital option)

---

## Instructions for Students:

### Step 1: Entity Identification (5 min)
- **Yellow sticky notes**
- Write all the "things" in TaskFlow
- One entity per note
- Think: What are the main objects/nouns in the system?

**Examples to get you started:**
- User
- Team  
- Project
- Task
- ?
- ?
- ?

**Your Entities:**
```
1. ________________
2. ________________  
3. ________________
4. ________________
5. ________________
6. ________________
7. ________________
8. ________________
```

### Step 2: Service Identification (5 min) 
- **Blue sticky notes**
- Write all the "actions" or "processes"
- Examples: "Assign Task", "Create Project"
- Think: What are the main actions/verbs in the system?

**Examples to get you started:**
- Assign Task
- Create Project
- Join Team
- ?
- ?
- ?

**Your Services:**
```
1. ________________
2. ________________
3. ________________
4. ________________
5. ________________
6. ________________
7. ________________
8. ________________
```

### Step 3: Event Identification (5 min)
- **Pink sticky notes**  
- Write all the "happenings"
- Examples: "Task Completed", "Team Member Added"
- Think: What are the important moments/events in the system?

**Examples to get you started:**
- Task Completed
- Team Member Added
- Project Deadline Missed
- ?
- ?
- ?

**Your Events:**
```
1. ________________
2. ________________
3. ________________
4. ________________
5. ________________
6. ________________
7. ________________
8. ________________
```

### Step 4: Domain Grouping (5 min)
- Group related notes together
- Draw boundaries around groups
- Name each domain

**Domain Grouping Template:**
```
Domain 1: ________________
├── Entities: 
├── Services:
└── Events:

Domain 2: ________________
├── Entities:
├── Services:
└── Events:

Domain 3: ________________
├── Entities:
├── Services:
└── Events:

Domain 4: ________________
├── Entities:
├── Services:
└── Events:

Domain 5: ________________
├── Entities:
├── Services:
└── Events:
```

---

## Facilitator Questions:
*(Your instructor will ask these as they walk around)*

- "Why did you put User and Team together?"
- "What happens when this event occurs?"
- "How do these domains communicate?"
- "Could this domain exist independently?"
- "What would break if we separated these?"

---

## Expected Domain Boundaries:
*(Don't peek until after your grouping!)*

<details>
<summary>Click to reveal suggested domains</summary>

### Team Management Domain
- **Entities**: Team, Member, Role
- **Services**: Add Member, Remove Member, Change Role
- **Events**: Member Joined, Member Left, Role Changed

### Project Planning Domain
- **Entities**: Project, Milestone, Requirement
- **Services**: Create Project, Set Milestone, Define Requirements
- **Events**: Project Created, Milestone Reached, Deadline Changed

### Task Execution Domain
- **Entities**: Task, Assignment, Comment
- **Services**: Create Task, Assign Task, Update Status
- **Events**: Task Created, Task Assigned, Task Completed

### User Identity Domain
- **Entities**: User, Permission, Session
- **Services**: Login, Logout, Grant Permission
- **Events**: User Logged In, Permission Granted, Session Expired

### Communication Domain
- **Entities**: Notification, Message, Subscription
- **Services**: Send Notification, Subscribe, Unsubscribe
- **Events**: Notification Sent, Message Delivered, Subscription Created

</details>

---

## Reflection Questions:

1. **How many domains did you identify?**
   ```
   Count: 
   ```

2. **Which domain seems most complex?**
   ```
   Domain:
   Reason:
   ```

3. **Which domains could be separate microservices?**
   ```
   Independent domains:
   ```

4. **Where do you see the most dependencies between domains?**
   ```
   Dependencies:
   ```

---

**Time Limit**: 20 minutes total
**Outcome**: Clear domain boundaries for TaskFlow API design 