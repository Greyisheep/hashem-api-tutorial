using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskFlow.API.Models;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;

    public AuthController(ILogger<AuthController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Authenticate user and get JWT token
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>JWT token and user information</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public ActionResult<ApiResponse<object>> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for user: {Username}", request.Username);

            // Simple authentication for demo purposes
            // In production, this would validate against a database
            if (request.Username == "admin@taskflow.com" && request.Password == "admin123")
            {
                var token = GenerateJwtToken(request.Username, "Admin");
                
                var response = new
                {
                    token = token,
                    refreshToken = Guid.NewGuid().ToString(),
                    expiresIn = 3600,
                    user = new
                    {
                        username = request.Username,
                        role = "Admin"
                    }
                };

                _logger.LogInformation("Login successful for user: {Username}", request.Username);
                return Ok(ApiResponse.SuccessResponse(response, "Login successful"));
            }

            _logger.LogWarning("Login failed for user: {Username}", request.Username);
            return Unauthorized(ApiResponse.ErrorResponse("INVALID_CREDENTIALS", "Invalid username or password"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", request.Username);
            return StatusCode(500, ApiResponse.ErrorResponse("LOGIN_ERROR", "An error occurred during login"));
        }
    }

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New JWT token</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public ActionResult<ApiResponse<object>> Refresh([FromBody] RefreshTokenRequest request)
    {
        try
        {
            _logger.LogInformation("Token refresh requested");

            // In production, validate the refresh token against a database
            var token = GenerateJwtToken("admin@taskflow.com", "Admin");
            
            var response = new
            {
                token = token,
                refreshToken = Guid.NewGuid().ToString(),
                expiresIn = 3600
            };

            return Ok(ApiResponse.SuccessResponse(response, "Token refreshed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, ApiResponse.ErrorResponse("REFRESH_ERROR", "An error occurred during token refresh"));
        }
    }

    /// <summary>
    /// Logout user
    /// </summary>
    /// <returns>Logout confirmation</returns>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<object>> Logout()
    {
        try
        {
            _logger.LogInformation("Logout requested for user: {Username}", User.Identity?.Name);

            // In production, invalidate the refresh token
            return Ok(ApiResponse.SuccessResponse(new { }, "Logout successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, ApiResponse.ErrorResponse("LOGOUT_ERROR", "An error occurred during logout"));
        }
    }

    private string GenerateJwtToken(string username, string role)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"]!);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}

public record LoginRequest
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public record RefreshTokenRequest
{
    public string RefreshToken { get; init; } = string.Empty;
} 