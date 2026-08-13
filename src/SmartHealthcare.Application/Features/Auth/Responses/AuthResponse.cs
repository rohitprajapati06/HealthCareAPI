using System;
using System.Collections.Generic;
using System.Text;

namespace SmartHealthcare.Application.Features.Auth.Responses
{
    public sealed class AuthResponse
    {
        public string AccessToken { get; init; } = string.Empty;

        public string RefreshToken { get; init; } = string.Empty;

        public DateTime ExpiresAt { get; init; }

        public string Roles { get; init; } = string.Empty;
    }
}
