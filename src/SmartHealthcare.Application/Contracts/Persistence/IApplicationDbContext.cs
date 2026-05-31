using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartHealthcare.Application.Contracts.Persistence
{
    public interface IApplicationDbContext
    {
        DbSet<RefreshToken> RefreshTokens { get; }

        DbSet<PatientProfile> PatientProfiles { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
