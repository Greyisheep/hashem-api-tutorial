# Google OAuth Configuration Guide

## Overview
This guide will help you properly configure Google OAuth for the TaskFlow API to resolve the "Invalid Origin" and "redirect_uri_mismatch" errors.

## Current Issue
You're getting `redirect_uri_mismatch` errors because the frontend runs on **port 3000**, not 7001!

## Step-by-Step Configuration

### 1. Google Cloud Console Setup

#### Navigate to Google Cloud Console
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Select your project (test-webintel)
3. Go to **APIs & Services** > **Credentials**
4. Find your OAuth 2.0 Client ID and click on it

#### Configure OAuth 2.0 Client ID

**For "Authorized JavaScript origins" (Frontend Origins):**
Add these base URLs only:
```
http://localhost:3000  ← FRONTEND
https://localhost:3000 ← FRONTEND
http://localhost:7001  ← API
https://localhost:7001 ← API
```

**For "Authorized redirect URIs" (Backend Callbacks):**
Add these full callback URLs:
```
http://localhost:7001/api/auth/google-callback  ← API PORT
https://localhost:7001/api/auth/google-callback ← API PORT
```

### 2. Environment Configuration

Update your `.env` file with the correct Google OAuth credentials:

```bash
# Google OAuth Configuration
GOOGLE_CLIENT_ID=your-google-client-id-here
GOOGLE_CLIENT_SECRET=your-google-client-secret-here
```

### 3. API Configuration

The API is already configured with:
- Callback path: `/api/auth/google-callback`
- PKCE enabled for security
- Proper CORS origins configured
- Development-friendly cookie settings

### 4. Testing the Configuration

#### Test the OAuth Flow
1. Start your API: `./start.sh`
2. Start your frontend: `python serve-frontend.py`
3. Navigate to: `http://localhost:3000`
4. Click "Login with Google"
5. You should be redirected to Google's consent screen
6. After authorization, you should be redirected back to frontend with token

#### Verify CORS
Test from your frontend:
```javascript
// Test OAuth login
window.location.href = 'http://localhost:7001/api/auth/google-login';
```

### 5. Common Issues and Solutions

#### Issue: "redirect_uri_mismatch" Error
**Cause:** Redirect URI in Google Console doesn't match API callback
**Solution:** 
- Add exact callback URL to "Authorized redirect URIs"
- Remove callback URLs from "Authorized JavaScript origins"
- Wait 2-3 minutes for Google's changes to propagate

#### Issue: "Invalid Origin" Error
**Cause:** Adding callback URLs to JavaScript origins
**Solution:** 
- Remove callback URLs from "Authorized JavaScript origins"
- Add only base URLs to JavaScript origins
- Add full callback URLs to "Authorized redirect URIs"

#### Issue: CORS Errors
**Cause:** Frontend origin not in CORS policy
**Solution:** The API has been updated to include all your domains in the CORS policy

#### Issue: Cookie Issues in Development
**Cause:** HTTPS requirements in development
**Solution:** The API is configured to allow HTTP cookies in development mode

### 6. Security Considerations

#### Production Deployment
For production, ensure:
1. Use HTTPS only
2. Update CORS origins to production domains
3. Use secure cookie settings
4. Implement proper session management

#### Environment Variables
Never commit `.env` files to version control. Use:
- `.env.example` for templates
- Environment variables in production
- Secrets management for sensitive data

### 7. Troubleshooting

#### Check API Logs
```bash
# View API logs
docker logs taskflow-api-dotnet-api-1
```

#### Test OAuth Endpoints
```bash
# Test login endpoint
curl -X GET "http://localhost:7001/api/auth/google-login"

# Test callback endpoint (requires OAuth flow)
curl -X GET "http://localhost:7001/api/auth/google-callback"
```

#### Verify Configuration
```bash
# Check if API is running
curl -X GET "http://localhost:7001/health"
```

## Quick Fix Summary

1. **Remove these from "Authorized JavaScript origins":**
   - ❌ `http://localhost:3000/api/auth/google-callback`
   - ❌ `https://localhost:3000/api/auth/google-callback`
   - ❌ `http://localhost:7001/api/auth/google-callback`
   - ❌ `https://localhost:7001/api/auth/google-callback`

2. **Add these to "Authorized JavaScript origins":**
   - ✅ `http://localhost:3000`  ← **FRONTEND**
   - ✅ `https://localhost:3000` ← **FRONTEND**
   - ✅ `http://localhost:7001`  ← **API**
   - ✅ `https://localhost:7001` ← **API**

3. **Add these to "Authorized redirect URIs":**
   - ✅ `http://localhost:7001/api/auth/google-callback`  ← **API PORT**
   - ✅ `https://localhost:7001/api/auth/google-callback` ← **API PORT**

4. **Wait 2-3 minutes** for Google's changes to propagate

5. **Test the OAuth flow** again

## OAuth Flow Summary

1. **User clicks "Login with Google"** on `http://localhost:3000` (frontend)
2. **Frontend redirects** to `http://localhost:7001/api/auth/google-login` (API)
3. **API redirects** to Google OAuth
4. **Google redirects back** to `http://localhost:7001/api/auth/google-callback` (API)
5. **API processes** OAuth response and redirects to frontend with token
6. **Frontend extracts** token and updates UI

This should resolve your Google OAuth configuration issues! 