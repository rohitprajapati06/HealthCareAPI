using System;
using System.Collections.Generic;
using System.Text;

namespace SmartHealthcare.Application.Common.Settings
{
    public class JwtSettings
    {
        public string Secret { get; init; } = string.Empty;

        public string Issuer { get; init; } = string.Empty;

        public string Audience { get; init; } = string.Empty;

        public int AccessTokenExpirationMinutes { get; init; }

        public int RefreshTokenExpirationDays { get; init; }

    }
}
