using Microsoft.AspNetCore.Http;
using SmartHealthcare.Application.Contracts.Identity;
using System.Security.Claims;

namespace SmartHealthcare.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                var userId = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);


                if (Guid.TryParse(userId, out var id))
                {
                    return id;
                }

                return null;
            }
        }

        public IReadOnlyList<string> Roles =>
            httpContextAccessor.HttpContext?.User?
                .FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList()
            ?? new List<string>();

        public bool IsInRole(string role) =>
            httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }
}