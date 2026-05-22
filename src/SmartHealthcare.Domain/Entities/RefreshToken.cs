using SmartHealthcare.Domain.Common;

namespace SmartHealthcare.Domain.Entities
{
    public class RefreshToken: BaseEntity
    {

        public Guid UserId { get; set; }

        public ApplicationUser User { get; set; }

        public string Token { get; set; }

        public DateTime ExpiryDate { get; set; }

        public bool IsRevoked { get; set; }

        public string? CreatedByIp { get; set; }



    }
}
