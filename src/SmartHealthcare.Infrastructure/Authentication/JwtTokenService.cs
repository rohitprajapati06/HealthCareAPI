using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartHealthcare.Application.Common.Settings;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.DTOs;
using SmartHealthcare.Application.Features.Auth.Responses;
using SmartHealthcare.Domain.Entities;
using System;
using System.Collections.Generic;
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

        public JwtTokenService(IOptions<JwtSettings> jwtsettings , UserManager<ApplicationUser> userManager )
        {

            this.jwtsettings = jwtsettings.Value;
            this.userManager = userManager;
        }
        

        public async Task<AuthResponse> GenerateTokenAsync(ApplicationUser user)
        {
            var roles = await userManager.GetRolesAsync( user );

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
    }
}
