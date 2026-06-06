
namespace SmartHealthcare.Application.Features.Auth.Responses
{
    public class CurrentUserResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public IList<string> Roles { get; set; } = new List<string>();

    }
}
