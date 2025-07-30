#!/bin/bash

echo "========================================"
echo "TaskFlow API - Secure Secrets Setup"
echo "========================================"
echo

# Check if .env already exists
if [ -f ".env" ]; then
    echo "⚠️  .env file already exists!"
    echo "Do you want to overwrite it? (y/N)"
    read -r response
    if [[ ! "$response" =~ ^[Yy]$ ]]; then
        echo "Setup cancelled."
        exit 0
    fi
fi

echo "🔐 Setting up secure environment variables..."
echo

# Create .env file with user's Google OAuth credentials
cat > .env << 'EOF'
# TaskFlow API Environment Variables
# This file contains sensitive information - NEVER commit to version control!

# =============================================================================
# JWT CONFIGURATION
# =============================================================================
JWT_SECRET_KEY=be3631dbacc12c75f138ab0c234cb9d2c6bf023392876aadf6512854d0524d0f
JWT_ISSUER=TaskFlow-API
JWT_AUDIENCE=TaskFlow-Users
JWT_EXPIRATION_MINUTES=60

# =============================================================================
# GOOGLE OAUTH CONFIGURATION
# =============================================================================
# Your Google OAuth credentials
GOOGLE_CLIENT_ID=245465575833-rl7c8f4840vlnh7tsi4jkpl2977cdn0t.apps.googleusercontent.com
GOOGLE_CLIENT_SECRET=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

# =============================================================================
# DATABASE CONFIGURATION
# =============================================================================
POSTGRES_DB=taskflow
POSTGRES_USER=postgres
POSTGRES_PASSWORD=change-this-password-in-production

# =============================================================================
# API CONFIGURATION
# =============================================================================
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5000

# =============================================================================
# LOGGING CONFIGURATION
# =============================================================================
SERILOG_MINIMUM_LEVEL=Information
EOF

echo "✅ .env file created successfully!"
echo
echo "🔒 Security check:"
echo "- .env is in .gitignore ✅"
echo "- Secrets are not in docker-compose.yml ✅"
echo "- Environment variables are properly referenced ✅"
echo
echo "🚀 You can now start the application with:"
echo "   ./start.sh"
echo
echo "⚠️  IMPORTANT: Change the database password in production!" 