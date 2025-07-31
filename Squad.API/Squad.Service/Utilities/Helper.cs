using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Squad.Service.Utilities
{
    public static class Helper
    {
        private static HttpContent CreateHttpContent<T>(T data)
        {
            var jsonContent = JsonSerializer.Serialize(data);
            return new StringContent(jsonContent, Encoding.UTF8, "application/json");
        }

        public static async Task<string> RequestBankService<T>(string url, HttpMethod method, T content = default)
        {
            using var client = new HttpClient();

            var apiKey = Environment.GetEnvironmentVariable("BANK_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("API Key is missing or not set.");

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Authorization", apiKey);

            HttpResponseMessage response;

            switch (method.Method)
            {
                case "GET":
                    response = await client.GetAsync(url);
                    break;
                case "POST":
                    response = await client.PostAsync(url, CreateHttpContent(content));
                    break;
                case "PUT":
                    response = await client.PutAsync(url, CreateHttpContent(content));
                    break;
                case "DELETE":
                    response = await client.DeleteAsync(url);
                    break;
                default:
                    throw new NotSupportedException($"HTTP method {method} is not supported");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public static string GenerateJwtToken(string email, string roleName)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim("email", email),
                new Claim(ClaimTypes.Role, roleName)
            };
            var token = new JwtSecurityToken(
                Environment.GetEnvironmentVariable("JWT_ISSUER"),
                Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds,
                claims: claims
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static T DecryptAndDeserialize<T>(string cipherText)
        {
            try
            {
                var decryptedJson = Cipher.DecryptResponse(cipherText);

                if (string.IsNullOrWhiteSpace(decryptedJson))
                    throw new Exception("Decryption returned an empty or null string.");

                return JsonSerializer.Deserialize<T>(decryptedJson);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Decryption succeeded, but deserialization failed.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to decrypt and deserialize the input.", ex);
            }
        }


    }
}
