using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Configuration;

namespace Blazor
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        //private readonly TokenService _tokenService;
        public readonly TokenService _tokenService;
        private readonly ILogger<CustomAuthenticationStateProvider> _logger;
        private readonly IConfiguration _configuration;

        public CustomAuthenticationStateProvider(TokenService tokenService, ILogger<CustomAuthenticationStateProvider> logger, IConfiguration configuration)
        {
            _tokenService = tokenService;
            _configuration = configuration;
            _logger = logger;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            _logger.LogInformation($"GetAuthenticationStateAsync is called");
            var identity = new ClaimsIdentity();
            if (!string.IsNullOrEmpty(_tokenService.JwtToken))
            {
                _logger.LogInformation($"Retrieved JWT token: {_tokenService.JwtToken}");
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])), // Use the same key that was used to create the token

                    ValidateIssuer = false,
                    ValidIssuer = "Your-Issuer", // The issuer you expect

                    ValidateAudience = false,
                    ValidAudience = "Your-Audience", // The audience you expect

                    ValidateLifetime = true, // To validate token expiration

                    ClockSkew = TimeSpan.Zero // You can allow some clock skew if necessary
                };
                try
                {
                    SecurityToken validatedToken;
                    var principal = tokenHandler.ValidateToken(_tokenService.JwtToken, validationParameters, out validatedToken);
                    var jwtToken = validatedToken as JwtSecurityToken;

                    if (jwtToken != null)
                    {
                        var claims = jwtToken.Claims;
                        identity = new ClaimsIdentity(claims, "jwt");
                        _logger.LogInformation("User is authorized with token.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Token validation failed: {ex.Message}");
                }
            }
            else
            {
                _logger.LogInformation("No token found. User is not authorized.");
            }

            var user = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }

        public void NotifyUserAuthentication(string token)
        {
            _logger.LogInformation($"NotifyUserAuthentication is called");
            var tokenHandler = new JwtSecurityTokenHandler();
            var validatedToken = tokenHandler.ReadJwtToken(token);
            var claims = validatedToken.Claims;
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public void NotifyUserLogout()
        {
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }
    }

}
