@echo off
setlocal enabledelayedexpansion

echo 🔐 TaskFlow API OAuth Setup
echo ============================
echo.

REM Check if .env file exists
if exist ".env" (
    echo ⚠️  .env file already exists!
    echo    If you want to reconfigure OAuth, please backup your current .env file first.
    echo.
    set /p "continue=Do you want to continue? (y/N): "
    if /i not "!continue!"=="y" (
        echo Setup cancelled.
        exit /b 0
    )
)

echo 📋 Setting up Google OAuth Configuration...
echo.

REM Create .env file from template
if not exist ".env" (
    echo 📝 Creating .env file from template...
    copy env.example .env >nul
    echo ✅ .env file created
) else (
    echo 📝 .env file already exists, updating OAuth configuration...
)

echo.
echo 🔧 Google OAuth Configuration
echo =============================
echo.
echo To configure Google OAuth, you need to:
echo.
echo 1. Go to Google Cloud Console: https://console.cloud.google.com/
echo 2. Create or select your project
echo 3. Go to APIs ^& Services ^> Credentials
echo 4. Create an OAuth 2.0 Client ID
echo.
echo 📝 Configure these settings in Google Cloud Console:
echo.
echo Authorized JavaScript origins:
echo   ✅ http://localhost:3000
echo   ✅ https://localhost:3000
echo   ✅ http://localhost:7001
echo   ✅ https://localhost:7001
echo.
echo Authorized redirect URIs:
echo   ✅ http://localhost:7001/api/auth/google-callback
echo   ✅ https://localhost:7001/api/auth/google-callback
echo.

REM Get OAuth credentials from user
echo 🔑 Enter your Google OAuth credentials:
echo.

set /p "google_client_id=Google Client ID: "
set /p "google_client_secret=Google Client Secret: "

REM Update .env file with OAuth credentials
if exist ".env" (
    REM Create temporary file with updated values
    powershell -Command "(Get-Content .env) -replace 'GOOGLE_CLIENT_ID=.*', 'GOOGLE_CLIENT_ID=%google_client_id%' | Set-Content .env"
    powershell -Command "(Get-Content .env) -replace 'GOOGLE_CLIENT_SECRET=.*', 'GOOGLE_CLIENT_SECRET=%google_client_secret%' | Set-Content .env"
    
    echo ✅ OAuth credentials updated in .env file
) else (
    echo ❌ Error: .env file not found
    exit /b 1
)

echo.
echo 🎉 OAuth Setup Complete!
echo ========================
echo.
echo Next steps:
echo 1. Make sure your .env file is in .gitignore (it should be)
echo 2. Start the API: start.bat
echo 3. Start the frontend: python serve-frontend.py
echo 4. Test OAuth login at: http://localhost:3000
echo.
echo 📚 For detailed setup instructions, see: GOOGLE-OAUTH-SETUP.md
echo.
echo ⚠️  Security Reminder:
echo    - Never commit .env files to version control
echo    - Keep your OAuth credentials secure
echo    - Use different credentials for development and production
echo.

pause 