
using MediatR;
using SmartHealthcare.Application.Features.Auth.Responses;

namespace SmartHealthcare.Application.Features.Auth.Queries.GetCurrentUser
{
    public class GetCurrentUserQuery : IRequest<CurrentUserResponse>
    {

    }
}
