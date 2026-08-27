using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartHealthcare.Application.Common.Settings;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Auth.Responses;
using SmartHealthcare.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SmartHealthcare.Infrastructure.Authentication
{
    public class JwtTokenService:IJwtTokenService
    {
        private readonly JwtSettings jwtsettings;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IApplicationDbContext context;
        private readonly ILogger<JwtTokenService> logger;

        public JwtTokenService(IOptions<JwtSettings> jwtsettings , UserManager<ApplicationUser> userManager ,IApplicationDbContext context , ILogger<JwtTokenService> logger)
        {

            this.jwtsettings = jwtsettings.Value;
            this.userManager = userManager;
            this.context = context;
            this.logger = logger;
        }
        

        public async Task<AuthResponse> GenerateTokenAsync(ApplicationUser user,CancellationToken cancellationToken = default)
        {
            var roles = await userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub , user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti , Guid.NewGuid().ToString()),

                new Claim(ClaimTypes.NameIdentifier , user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtsettings.Secret));
            var credentials = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(jwtsettings.AccessTokenExpirationMinutes);
            var token = new JwtSecurityToken(
                  
                issuer: jwtsettings.Issuer,
                audience : jwtsettings.Audience,
                claims : claims,
                expires: expires,
                signingCredentials: credentials
                );
               

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = GenerateRefreshToken();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expires,
                Roles = roles.FirstOrDefault() ?? ""
            };

        }

        private static string GenerateRefreshToken()
        {
            var randombytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();

            rng.GetBytes(randombytes);

            return Convert.ToBase64String(randombytes);
        }

        public async Task<AuthResponse> RefreshTokenAsync(string refreshtoken , CancellationToken cancellationToken = default)
        {
            var storedrefreshtoken = await context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshtoken, cancellationToken);

            if(storedrefreshtoken == null)
            {
                throw new UnauthorizedAccessException("Invalid Refresh Token");
            }

            if (storedrefreshtoken.IsRevoked)
            {
                throw new UnauthorizedAccessException("refresh Token Revoked");
            }

            if(storedrefreshtoken.ExpiryDate <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Refresh Token is Expired");
            }

            var user = await userManager.FindByIdAsync(storedrefreshtoken.UserId.ToString());

            if(user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            var authResponse = await GenerateTokenAsync(user);

            storedrefreshtoken.IsRevoked = true;

            var  newRefreshtoken = new RefreshToken
            {
                UserId = user.Id,
                Token = authResponse.RefreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            await context.RefreshTokens.AddAsync(newRefreshtoken,cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return authResponse;
        }

        public async Task LogoutAsync(string refreshtoken, CancellationToken cancellationToken = default)
        {
            var token = await context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshtoken,cancellationToken);

            if(token == null)
            {
                throw new UnauthorizedAccessException("Refresh Token Not Found");
            }

            
            token.IsRevoked = true;
            
            logger.LogInformation("Refresh Token is revoked for user {UserId}",token.UserId);

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
