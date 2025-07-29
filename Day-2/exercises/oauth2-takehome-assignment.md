# OAuth 2.0 Take-Home Assignment

## 🎯 Assignment Objectives
- Implement OAuth 2.0 authorization code flow
- Integrate with Google/GitHub OAuth providers
- Create user accounts from OAuth data
- Generate JWT tokens for your API
- Apply security best practices

## ⏰ Estimated Time: 2-3 hours

---

## 🚀 OAuth 2.0 Overview

### What is OAuth 2.0?
OAuth 2.0 is an authorization framework that allows third-party applications to access user resources without sharing credentials.

### Authorization Code Flow (Recommended)
```
1. User clicks "Login with Google"
2. Redirect to Google OAuth
3. User authorizes your app
4. Google redirects back with authorization code
5. Your app exchanges code for access token
6. Use access token to get user info
7. Create/update user in your database
8. Generate JWT token for your API
```

---

## 🔧 Setup Instructions

### Step 1: Choose OAuth Provider

#### Option A: Google OAuth 2.0
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create new project or select existing
3. Enable Google+ API
4. Create OAuth 2.0 credentials
5. Add authorized redirect URIs:
   - `http://localhost:5000/auth/google/callback` (development)
   - `https://yourdomain.com/auth/google/callback` (production)

#### Option B: GitHub OAuth 2.0
1. Go to [GitHub Developer Settings](https://github.com/settings/developers)
2. Create new OAuth App
3. Set Authorization callback URL:
   - `http://localhost:5000/auth/github/callback` (development)
   - `https://yourdomain.com/auth/github/callback` (production)

### Step 2: Install Required Packages

```bash
# For Google OAuth
dotnet add package Google.Apis.Auth

# For GitHub OAuth
dotnet add package Octokit

# For HTTP client
dotnet add package Microsoft.Extensions.Http
```

---

## 🏗️ Implementation Guide

### Step 1: OAuth Configuration

#### appsettings.json
```json
{
  "OAuth": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret",
      "RedirectUri": "http://localhost:5000/auth/google/callback"
    },
    "GitHub": {
      "ClientId": "your-github-client-id",
      "ClientSecret": "your-github-client-secret",
      "RedirectUri": "http://localhost:5000/auth/github/callback"
    }
  }
}
```

### Step 2: OAuth Service Implementation

#### Google OAuth Service
```csharp
// TaskFlow.Infrastructure/Services/GoogleOAuthService.cs
public interface IGoogleOAuthService
{
    string GetAuthorizationUrl();
    Task<OAuthUserInfo> ExchangeCodeForUserInfoAsync(string code);
}

public class GoogleOAuthService : IGoogleOAuthService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    
    public GoogleOAuthService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }
    
    public string GetAuthorizationUrl()
    {
        var clientId = _configuration["OAuth:Google:ClientId"];
        var redirectUri = _configuration["OAuth:Google:RedirectUri"];
        
        var queryParams = new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["redirect_uri"] = redirectUri,
            ["response_type"] = "code",
            ["scope"] = "openid email profile",
            ["access_type"] = "offline"
        };
        
        var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        return $"https://accounts.google.com/o/oauth2/v2/auth?{queryString}";
    }
    
    public async Task<OAuthUserInfo> ExchangeCodeForUserInfoAsync(string code)
    {
        var clientId = _configuration["OAuth:Google:ClientId"];
        var clientSecret = _configuration["OAuth:Google:ClientSecret"];
        var redirectUri = _configuration["OAuth:Google:RedirectUri"];
        
        // Exchange authorization code for access token
        var tokenRequest = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("redirect_uri", redirectUri)
        });
        
        var tokenResponse = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequest);
        var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
        var tokenData = JsonSerializer.Deserialize<GoogleTokenResponse>(tokenContent);
        
        // Get user info using access token
        var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo");
        userInfoRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenData.AccessToken);
        
        var userInfoResponse = await _httpClient.SendAsync(userInfoRequest);
        var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(userInfoContent);
        
        return new OAuthUserInfo
        {
            Id = userInfo.Id,
            Email = userInfo.Email,
            Name = userInfo.Name,
            Picture = userInfo.Picture,
            Provider = "Google"
        };
    }
}

public class GoogleTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
    
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }
    
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

public class GoogleUserInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("email")]
    public string Email { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("picture")]
    public string Picture { get; set; }
}
```

#### GitHub OAuth Service
```csharp
// TaskFlow.Infrastructure/Services/GitHubOAuthService.cs
public interface IGitHubOAuthService
{
    string GetAuthorizationUrl();
    Task<OAuthUserInfo> ExchangeCodeForUserInfoAsync(string code);
}

public class GitHubOAuthService : IGitHubOAuthService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    
    public GitHubOAuthService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }
    
    public string GetAuthorizationUrl()
    {
        var clientId = _configuration["OAuth:GitHub:ClientId"];
        var redirectUri = _configuration["OAuth:GitHub:RedirectUri"];
        
        var queryParams = new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["redirect_uri"] = redirectUri,
            ["scope"] = "user:email"
        };
        
        var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        return $"https://github.com/login/oauth/authorize?{queryString}";
    }
    
    public async Task<OAuthUserInfo> ExchangeCodeForUserInfoAsync(string code)
    {
        var clientId = _configuration["OAuth:GitHub:ClientId"];
        var clientSecret = _configuration["OAuth:GitHub:ClientSecret"];
        
        // Exchange authorization code for access token
        var tokenRequest = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("code", code)
        });
        
        var tokenResponse = await _httpClient.PostAsync("https://github.com/login/oauth/access_token", tokenRequest);
        var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
        var tokenData = JsonSerializer.Deserialize<GitHubTokenResponse>(tokenContent);
        
        // Get user info using access token
        var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
        userInfoRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenData.AccessToken);
        userInfoRequest.Headers.Add("User-Agent", "TaskFlow-API");
        
        var userInfoResponse = await _httpClient.SendAsync(userInfoRequest);
        var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<GitHubUserInfo>(userInfoContent);
        
        return new OAuthUserInfo
        {
            Id = userInfo.Id.ToString(),
            Email = userInfo.Email,
            Name = userInfo.Name,
            Picture = userInfo.AvatarUrl,
            Provider = "GitHub"
        };
    }
}

public class GitHubTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
    
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }
}

public class GitHubUserInfo
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
    
    [JsonPropertyName("email")]
    public string Email { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; }
}
```

### Step 3: OAuth Controller

```csharp
// TaskFlow.API/Controllers/OAuthController.cs
[ApiController]
[Route("api/[controller]")]
public class OAuthController : ControllerBase
{
    private readonly IGoogleOAuthService _googleOAuthService;
    private readonly IGitHubOAuthService _githubOAuthService;
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    
    public OAuthController(
        IGoogleOAuthService googleOAuthService,
        IGitHubOAuthService githubOAuthService,
        IUserRepository userRepository,
        IJwtService jwtService)
    {
        _googleOAuthService = googleOAuthService;
        _githubOAuthService = githubOAuthService;
        _userRepository = userRepository;
        _jwtService = jwtService;
    }
    
    [HttpGet("google/login")]
    public IActionResult GoogleLogin()
    {
        var authUrl = _googleOAuthService.GetAuthorizationUrl();
        return Redirect(authUrl);
    }
    
    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string code)
    {
        try
        {
            var oauthUserInfo = await _googleOAuthService.ExchangeCodeForUserInfoAsync(code);
            var user = await GetOrCreateUserAsync(oauthUserInfo);
            var token = _jwtService.GenerateToken(user);
            
            return Ok(new { token, user = new { user.Id, user.Email, user.Name } });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "OAuth authentication failed", error = ex.Message });
        }
    }
    
    [HttpGet("github/login")]
    public IActionResult GitHubLogin()
    {
        var authUrl = _githubOAuthService.GetAuthorizationUrl();
        return Redirect(authUrl);
    }
    
    [HttpGet("github/callback")]
    public async Task<IActionResult> GitHubCallback([FromQuery] string code)
    {
        try
        {
            var oauthUserInfo = await _githubOAuthService.ExchangeCodeForUserInfoAsync(code);
            var user = await GetOrCreateUserAsync(oauthUserInfo);
            var token = _jwtService.GenerateToken(user);
            
            return Ok(new { token, user = new { user.Id, user.Email, user.Name } });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "OAuth authentication failed", error = ex.Message });
        }
    }
    
    private async Task<User> GetOrCreateUserAsync(OAuthUserInfo oauthUserInfo)
    {
        var existingUser = await _userRepository.GetByEmailAsync(Email.From(oauthUserInfo.Email));
        
        if (existingUser != null)
        {
            // Update user info if needed
            existingUser.UpdateOAuthInfo(oauthUserInfo.Name, oauthUserInfo.Picture);
            return await _userRepository.UpdateAsync(existingUser);
        }
        
        // Create new user
        var newUser = new User(
            Email.From(oauthUserInfo.Email),
            UserName.From(oauthUserInfo.Name),
            UserRole.Developer, // Default role
            oauthUserInfo.Picture
        );
        
        return await _userRepository.AddAsync(newUser);
    }
}

public class OAuthUserInfo
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string Picture { get; set; }
    public string Provider { get; set; }
}
```

### Step 4: Service Registration

```csharp
// Program.cs
builder.Services.AddHttpClient();
builder.Services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();
builder.Services.AddScoped<IGitHubOAuthService, GitHubOAuthService>();
```

---

## 🧪 Testing Your Implementation

### Test OAuth Flow
1. **Start your API**: `dotnet run`
2. **Visit login URL**: `http://localhost:5000/api/oauth/google/login`
3. **Complete OAuth flow**: Authorize with Google
4. **Check callback**: Should return JWT token
5. **Test with token**: Use JWT in API requests

### Test with Postman
```bash
# 1. Get OAuth URL
GET http://localhost:5000/api/oauth/google/login

# 2. Complete OAuth flow in browser
# 3. Use returned JWT token
GET http://localhost:5000/api/tasks
Authorization: Bearer <your-jwt-token>
```

---

## 🔒 Security Best Practices

### Implement These Security Measures:
1. **State Parameter**: Prevent CSRF attacks
2. **PKCE**: For public clients (mobile apps)
3. **Token Validation**: Always validate OAuth tokens
4. **User Verification**: Verify email addresses
5. **Rate Limiting**: Prevent abuse
6. **Secure Storage**: Store tokens securely
7. **Error Handling**: Don't expose sensitive info

### Example: State Parameter Implementation
```csharp
public string GetAuthorizationUrl()
{
    var state = GenerateSecureRandomString();
    HttpContext.Session.SetString("oauth_state", state);
    
    var queryParams = new Dictionary<string, string>
    {
        ["client_id"] = clientId,
        ["redirect_uri"] = redirectUri,
        ["response_type"] = "code",
        ["state"] = state, // Add state parameter
        ["scope"] = "openid email profile"
    };
    
    // ... rest of implementation
}

public async Task<OAuthUserInfo> ExchangeCodeForUserInfoAsync(string code, string state)
{
    var expectedState = HttpContext.Session.GetString("oauth_state");
    if (state != expectedState)
    {
        throw new SecurityException("Invalid state parameter");
    }
    
    // ... rest of implementation
}
```

---

## 🎯 Success Criteria

### Excellent Implementation (All of these):
- [ ] OAuth 2.0 flow working end-to-end
- [ ] User creation/update from OAuth data
- [ ] JWT token generation
- [ ] Security best practices implemented
- [ ] Error handling and validation
- [ ] Clean, maintainable code

### Good Implementation (Most of these):
- [ ] OAuth 2.0 flow working
- [ ] User creation from OAuth data
- [ ] JWT token generation
- [ ] Basic error handling
- [ ] Functional implementation

### Needs Improvement (Few of these):
- [ ] OAuth flow not working
- [ ] No user creation
- [ ] No JWT generation
- [ ] Poor error handling
- [ ] Security vulnerabilities

---

## 📚 Additional Resources

### Documentation:
- [Google OAuth 2.0 Documentation](https://developers.google.com/identity/protocols/oauth2)
- [GitHub OAuth Documentation](https://docs.github.com/en/developers/apps/building-oauth-apps)
- [OAuth 2.0 RFC](https://tools.ietf.org/html/rfc6749)

### Tutorials:
- [ASP.NET Core OAuth Tutorial](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/)
- [Google OAuth with .NET](https://developers.google.com/identity/sign-in/web/sign-in)
- [GitHub OAuth with .NET](https://docs.github.com/en/developers/apps/building-oauth-apps/authorizing-oauth-apps)

### Security Resources:
- [OAuth 2.0 Security Best Practices](https://tools.ietf.org/html/draft-ietf-oauth-security-topics)
- [OWASP OAuth 2.0 Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/OAuth_2_0_Cheat_Sheet.html)

---

## 🚀 Next Steps

### After Implementation:
1. **Test thoroughly**: Try different scenarios
2. **Add error handling**: Handle edge cases
3. **Implement security**: Add state parameter, PKCE
4. **Add logging**: Monitor OAuth flows
5. **Document**: Update API documentation

### Day 3 Preview:
"Tomorrow we'll add advanced security features and deploy your OAuth-enabled API to production!" 