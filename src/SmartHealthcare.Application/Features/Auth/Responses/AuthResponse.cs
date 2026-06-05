using System;
using System.Collections.Generic;
using System.Text;

namespace SmartHealthcare.Application.Features.Auth.Responses
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public string Roles { get; set; } = string.Empty;
    }
}
