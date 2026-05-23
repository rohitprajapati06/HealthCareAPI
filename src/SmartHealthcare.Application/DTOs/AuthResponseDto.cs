using System;
using System.Collections.Generic;
using System.Text;

namespace SmartHealthcare.Application.DTOs
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public DateTime ExpiresAt { get; set; }

        public string Roles { get; set; }
    }
}
