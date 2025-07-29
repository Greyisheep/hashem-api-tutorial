# 🚀 Student Setup Guide - TaskFlow API

## Quick Start (3 Commands!)

### Prerequisites
- Docker Desktop installed and running
- Git (to clone the repository)

### Step 1: Navigate to the Project
```bash
cd taskflow-api-dotnet
```

### Step 2: Start Everything (One Command!)
**Windows:**
```bash
start.bat
```

**Mac/Linux:**
```bash
chmod +x start.sh
./start.sh
```

### Step 3: Access the API
Open your browser and go to: **http://localhost:5000/swagger**

That's it! 🎉

---

## What Just Happened?

The `start.bat`/`start.sh` script automatically:
- ✅ Starts PostgreSQL database
- ✅ Starts Redis cache
- ✅ Starts Seq logging
- ✅ Starts pgAdmin (database management)
- ✅ Builds and starts the TaskFlow API
- ✅ Runs database migrations
- ✅ Sets up all environment variables

## Available Services

| Service | URL | Purpose |
|---------|-----|---------|
| **API** | http://localhost:5000 | Main API endpoints |
| **Swagger** | http://localhost:5000/swagger | Interactive API docs |
| **Health** | http://localhost:5000/health | API health check |
| **pgAdmin** | http://localhost:5050 | Database management |
| **Seq** | http://localhost:5341 | Log viewing |

## Testing the API

### Option 1: Swagger UI (Easiest)
1. Go to http://localhost:5000/swagger
2. Click "Authorize" → Enter: `Bearer your-jwt-token`
3. Test any endpoint directly

### Option 2: Postman
1. Import: `postman/TaskFlow-API.postman_collection.json`
2. Import: `postman/TaskFlow-API.postman_environment.json`
3. Run the "Login" request to get a token
4. Test all endpoints

### Option 3: curl
```bash
# Health check
curl http://localhost:5000/health

# Get all tasks
curl http://localhost:5000/api/tasks

# Create a task
curl -X POST http://localhost:5000/api/tasks \
  -H "Content-Type: application/json" \
  -d '{"title":"Test Task","description":"My first task"}'
```

## Database Access

**pgAdmin Login:**
- Email: `admin@taskflow.com`
- Password: `admin123`

**Direct Database:**
- Host: `localhost`
- Port: `5432`
- Database: `taskflow`
- Username: `postgres`
- Password: `password`

## Stopping the Services

Press `Ctrl+C` in the terminal where you ran the start script.

## Troubleshooting

### "Docker is not running"
- Start Docker Desktop
- Wait for it to fully initialize
- Try again

### "Port already in use"
- Stop any services using ports 5000, 5432, 5050, 5341
- Or change ports in `docker-compose.yml`

### "Database connection failed"
- Wait 30 seconds for PostgreSQL to fully start
- Check Docker logs: `docker-compose logs postgres`

### "Build failed"
- Check Docker logs: `docker-compose logs taskflow-api`
- Ensure you have .NET 8 SDK installed

## What You Can Do Now

✅ **Explore the API** - Use Swagger to see all endpoints
✅ **Test Authentication** - Login and get JWT tokens
✅ **CRUD Operations** - Create, read, update, delete tasks
✅ **Database Design** - Use pgAdmin to explore the schema
✅ **Follow Day 2 Workshops** - Use this API for hands-on exercises

## 🔒 Security Note

**Important**: This development setup includes hardcoded secrets (JWT keys, database passwords) in the `docker-compose.yml` file. This is **intentional for learning purposes** - it makes the setup easier for students.

**In production** (Day 3), we'll move these to:
- Environment variables (`.env` files)
- Azure Key Vault / AWS Secrets Manager
- Kubernetes secrets
- Secure configuration management

**Current setup is for learning only!** 🎓

---

**Ready for Day 2! 🎯**

The TaskFlow API demonstrates:
- Domain-Driven Design (DDD)
- Clean Architecture
- JWT Authentication
- CQRS Pattern
- Repository Pattern
- Entity Framework Core
- Docker containerization 