

using MediatR;
using Microsoft.AspNetCore.Identity;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.Auth.Responses;
using SmartHealthcare.Domain.Entities;

namespace SmartHealthcare.Application.Features.Auth.Queries.GetCurrentUser
{
    public class GetCurrentUserQueryHandler:IRequestHandler<GetCurrentUserQuery,CurrentUserResponse>
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ICurrentUserService currentUserService;

        public GetCurrentUserQueryHandler(UserManager<ApplicationUser> userManager , ICurrentUserService currentUserService)
        {
            this.userManager = userManager;
            this.currentUserService = currentUserService;
        }

        public async Task<CurrentUserResponse> Handle(GetCurrentUserQuery request , CancellationToken cancellationToken)
        {
            var userId = currentUserService.UserId;

            if (userId == null) {
                throw new UnauthorizedAccessException("User is not Authenticated");

            }

            var user = await userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            var roles = await userManager.GetRolesAsync(user);

            return new CurrentUserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Roles = roles
            };


            
        }
    }
}
