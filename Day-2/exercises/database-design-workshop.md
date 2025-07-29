# Database Design Workshop - dbdiagram.io

## 🎯 Workshop Objectives
- Design TaskFlow database schema using dbdiagram.io
- Understand domain-to-database mapping
- Practice database design best practices
- Create production-ready database schema

## ⏰ Workshop Timing: 60 minutes

---

## 🚀 Setup (5 minutes)

### Create dbdiagram.io Account
1. Go to [dbdiagram.io](https://dbdiagram.io)
2. Sign up with GitHub/Google (free account)
3. Create new diagram: "TaskFlow Database"

### Quick dbdiagram.io Tutorial
- **Tables**: Click "Add Table" or press `T`
- **Relationships**: Drag from foreign key to primary key
- **Notes**: Add comments with `//` syntax
- **Export**: Download as SQL, PDF, or image

---

## 🏗️ Database Design Principles (10 minutes)

### Domain-Driven Database Design
**Question**: "How do your domain entities become database tables?"

#### Key Principles:
1. **One Entity = One Table** (usually)
2. **Value Objects = Columns** (not separate tables)
3. **Aggregates = Tables with relationships**
4. **Domain Events = Audit tables** (optional)

#### TaskFlow Domain Entities (from `taskflow-api-dotnet/src/TaskFlow.Domain/Entities/`):
- **User** (Aggregate Root) - `User.cs`
- **Project** (Aggregate Root) - `Project.cs`
- **Task** (Aggregate Root) - `Task.cs`

#### TaskFlow Value Objects (from `taskflow-api-dotnet/src/TaskFlow.Domain/ValueObjects/`):
- **Email** - `Email.cs`
- **TaskTitle** - `TaskTitle.cs`
- **ProjectName** - `ProjectName.cs`
- **UserRole** - `UserRole.cs`
- **TaskStatus** - `TaskStatus.cs`

---

## 🎯 Hands-on Design (30 minutes)

### Step 1: Core Tables (15 minutes)
**Task**: Design the core TaskFlow tables based on the actual schema

#### Reference: `taskflow-api-dotnet/database/taskflow-database-design.dbml`

#### User Table
```sql
Table users {
  id varchar(36) [pk, note: 'GUID as primary key']
  email varchar(255) [unique, not null, note: 'User email (unique)']
  first_name varchar(100) [not null]
  last_name varchar(100) [not null]
  password_hash varchar(500) [not null, note: 'BCrypt hashed password']
  role varchar(50) [not null, note: 'Admin, ProjectManager, Developer, Viewer']
  status varchar(50) [not null, note: 'Active, Inactive, Suspended']
  created_at timestamptz [not null, default: `now()`]
  updated_at timestamptz [not null, default: `now()`]
  last_login_at timestamptz [null]
  
  indexes {
    email [unique]
    role
    status
    created_at
  }
}
```

#### Project Table
```sql
Table projects {
  id varchar(36) [pk]
  name varchar(200) [not null]
  description text [not null]
  status varchar(50) [not null, note: 'Planning, Active, Completed, Cancelled']
  owner_id varchar(36) [not null, ref: > users.id]
  created_at timestamptz [not null, default: `now()`]
  updated_at timestamptz [not null, default: `now()`]
  start_date timestamptz [null]
  end_date timestamptz [null]
  
  indexes {
    owner_id
    status
    created_at
    (start_date, end_date)
  }
}
```

#### Task Table
```sql
Table tasks {
  id varchar(36) [pk]
  title varchar(200) [not null]
  description text [not null]
  status varchar(50) [not null, note: 'pending, in_progress, completed, cancelled']
  priority varchar(50) [not null, default: 'medium', note: 'low, medium, high, urgent']
  assignee_id varchar(36) [null, ref: > users.id]
  project_id varchar(36) [not null, ref: > projects.id]
  created_by varchar(36) [not null, ref: > users.id]
  created_at timestamptz [not null, default: `now()`]
  updated_at timestamptz [not null, default: `now()`]
  due_date timestamptz [null]
  completed_at timestamptz [null]
  
  indexes {
    project_id
    assignee_id
    created_by
    status
    priority
    created_at
    due_date
    (project_id, status)
    (assignee_id, status)
  }
}
```

### Step 2: Relationships & Constraints (10 minutes)
**Task**: Add relationships and constraints

#### Project Members (Many-to-Many)
```sql
Table project_members {
  project_id varchar(36) [ref: > projects.id]
  user_id varchar(36) [ref: > users.id]
  added_at timestamptz [not null, default: `now()`]
  added_by varchar(36) [not null, ref: > users.id]
  
  indexes {
    (project_id, user_id) [pk]
    user_id
    added_at
  }
}
```

#### Comments Domain
```sql
Table comments {
  id varchar(36) [pk]
  content text [not null]
  task_id varchar(36) [not null, ref: > tasks.id]
  author_id varchar(36) [not null, ref: > users.id]
  created_at timestamptz [not null, default: `now()`]
  updated_at timestamptz [not null, default: `now()`]
  is_edited boolean [not null, default: false]
  
  indexes {
    task_id
    author_id
    created_at
    (task_id, created_at)
  }
}
```

### Step 3: Enums & Types (5 minutes)
**Task**: Define custom types based on value objects

```sql
Enum user_role {
  admin
  project_manager
  developer
  viewer
}

Enum project_status {
  planning
  active
  completed
  cancelled
}

Enum task_status {
  pending
  in_progress
  completed
  cancelled
}

Enum task_priority {
  low
  medium
  high
  urgent
}

Enum user_status {
  active
  inactive
  suspended
}
```

---

## 🔍 Schema Review & Optimization (15 minutes)

### Pair Review (10 minutes)
**Activity**: Review partner's schema against the reference

#### Review Checklist:
- [ ] All domain entities represented?
- [ ] Proper relationships defined?
- [ ] Appropriate indexes added?
- [ ] Data types appropriate?
- [ ] Constraints logical?

#### Key Questions:
- "How would this handle 10,000 tasks?"
- "What indexes would you add for performance?"
- "How do you handle soft deletes?"
- "What about audit trails?"

### Performance Optimization (5 minutes)
**Discussion**: "What indexes do we need?"

#### Recommended Indexes (from actual schema):
```sql
-- User queries
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_role ON users(role);
CREATE INDEX idx_users_status ON users(status);

-- Project queries
CREATE INDEX idx_projects_owner_id ON projects(owner_id);
CREATE INDEX idx_projects_status ON projects(status);
CREATE INDEX idx_projects_dates ON projects(start_date, end_date);

-- Task queries
CREATE INDEX idx_tasks_project_id ON tasks(project_id);
CREATE INDEX idx_tasks_assignee_id ON tasks(assignee_id);
CREATE INDEX idx_tasks_status ON tasks(status);
CREATE INDEX idx_tasks_priority ON tasks(priority);
CREATE INDEX idx_tasks_project_status ON tasks(project_id, status);
CREATE INDEX idx_tasks_assignee_status ON tasks(assignee_id, status);

-- Comment queries
CREATE INDEX idx_comments_task_id ON comments(task_id);
CREATE INDEX idx_comments_task_created ON comments(task_id, created_at);
```

---

## 🎯 Success Criteria

### Excellent Design (All of these):
- [ ] All domain entities properly represented
- [ ] Relationships correctly defined
- [ ] Appropriate indexes identified
- [ ] Performance considerations noted
- [ ] Schema follows best practices

### Good Design (Most of these):
- [ ] Core entities represented
- [ ] Basic relationships defined
- [ ] Some indexes identified
- [ ] Basic performance awareness
- [ ] Reasonable schema structure

### Needs Improvement (Few of these):
- [ ] Missing key entities
- [ ] Incorrect relationships
- [ ] No performance consideration
- [ ] Poor data types
- [ ] Missing constraints

---

## 📝 Facilitator Notes

### Common Issues to Watch For:
- **Over-normalization**: Too many small tables
- **Under-normalization**: Everything in one table
- **Missing relationships**: No foreign keys
- **Poor naming**: Inconsistent conventions
- **No indexes**: Performance problems

### Questions to Ask:
- "How would you query tasks by project?"
- "What happens when a user is deleted?"
- "How do you handle task assignments?"
- "What about project permissions?"

### Energy Management:
- **Celebrate good designs** - "Great relationship!"
- **Help with challenges** - "Let's think about this..."
- **Encourage questions** - "What's unclear?"
- **Build confidence** - "You're getting it!"

### Reference the Actual Code:
- **Domain Entities**: `taskflow-api-dotnet/src/TaskFlow.Domain/Entities/`
- **Value Objects**: `taskflow-api-dotnet/src/TaskFlow.Domain/ValueObjects/`
- **Database Schema**: `taskflow-api-dotnet/database/taskflow-database-design.dbml`
- **DDD Summary**: `taskflow-api-dotnet/DDD-IMPLEMENTATION-SUMMARY.md`

---

## 🚀 Next Steps

### Take-Home Assignment:
**Task**: Implement this database schema in your TaskFlow API
- Create Entity Framework migrations
- Add seed data
- Test relationships
- Document any changes needed

### Day 3 Preview:
"We'll use this database design for authentication and authorization implementation!" 