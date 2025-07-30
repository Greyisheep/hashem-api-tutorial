#!/bin/bash

# TaskFlow API OAuth Setup Script
# This script helps you configure Google OAuth for the TaskFlow API

set -e

echo "🔐 TaskFlow API OAuth Setup"
echo "============================"
echo ""

# Check if .env file exists
if [ -f ".env" ]; then
    echo "⚠️  .env file already exists!"
    echo "   If you want to reconfigure OAuth, please backup your current .env file first."
    echo ""
    read -p "Do you want to continue? (y/N): " -n 1 -r
    echo ""
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "Setup cancelled."
        exit 0
    fi
fi

echo "📋 Setting up Google OAuth Configuration..."
echo ""

# Create .env file from template
if [ ! -f ".env" ]; then
    echo "📝 Creating .env file from template..."
    cp env.example .env
    echo "✅ .env file created"
else
    echo "📝 .env file already exists, updating OAuth configuration..."
fi

echo ""
echo "🔧 Google OAuth Configuration"
echo "============================="
echo ""
echo "To configure Google OAuth, you need to:"
echo ""
echo "1. Go to Google Cloud Console: https://console.cloud.google.com/"
echo "2. Create or select your project"
echo "3. Go to APIs & Services > Credentials"
echo "4. Create an OAuth 2.0 Client ID"
echo ""
echo "📝 Configure these settings in Google Cloud Console:"
echo ""
echo "Authorized JavaScript origins:"
echo "  ✅ http://localhost:3000"
echo "  ✅ https://localhost:3000"
echo "  ✅ http://localhost:7001"
echo "  ✅ https://localhost:7001"
echo ""
echo "Authorized redirect URIs:"
echo "  ✅ http://localhost:7001/api/auth/google-callback"
echo "  ✅ https://localhost:7001/api/auth/google-callback"
echo ""

# Get OAuth credentials from user
echo "🔑 Enter your Google OAuth credentials:"
echo ""

read -p "Google Client ID: " google_client_id
read -p "Google Client Secret: " google_client_secret

# Update .env file with OAuth credentials
if [ -f ".env" ]; then
    # Use sed to update the .env file
    if [[ "$OSTYPE" == "darwin"* ]]; then
        # macOS
        sed -i '' "s/GOOGLE_CLIENT_ID=.*/GOOGLE_CLIENT_ID=$google_client_id/" .env
        sed -i '' "s/GOOGLE_CLIENT_SECRET=.*/GOOGLE_CLIENT_SECRET=$google_client_secret/" .env
    else
        # Linux
        sed -i "s/GOOGLE_CLIENT_ID=.*/GOOGLE_CLIENT_ID=$google_client_id/" .env
        sed -i "s/GOOGLE_CLIENT_SECRET=.*/GOOGLE_CLIENT_SECRET=$google_client_secret/" .env
    fi
    
    echo "✅ OAuth credentials updated in .env file"
else
    echo "❌ Error: .env file not found"
    exit 1
fi

echo ""
echo "🎉 OAuth Setup Complete!"
echo "========================"
echo ""
echo "Next steps:"
echo "1. Make sure your .env file is in .gitignore (it should be)"
echo "2. Start the API: ./start.sh"
echo "3. Start the frontend: python serve-frontend.py"
echo "4. Test OAuth login at: http://localhost:3000"
echo ""
echo "📚 For detailed setup instructions, see: GOOGLE-OAUTH-SETUP.md"
echo ""
echo "⚠️  Security Reminder:"
echo "   - Never commit .env files to version control"
echo "   - Keep your OAuth credentials secure"
echo "   - Use different credentials for development and production"
echo "" 