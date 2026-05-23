using System;
using System.Collections.Generic;
using System.Text;

namespace SmartHealthcare.Application.Configurations
{
    public class JwtSettings
    {
        public string Secret { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public int AccessTokenExpirationMinutes { get; set; }

        public int RefreshTokenExpirationDays { get; set; }

    }
}
