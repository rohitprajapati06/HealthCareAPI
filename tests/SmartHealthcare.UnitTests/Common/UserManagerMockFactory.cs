using Microsoft.AspNetCore.Identity;
using Moq;
using SmartHealthcare.Domain.Entities;

namespace SmartHealthcare.UnitTests.Common
{
    /// <summary>
    /// UserManager&lt;TUser&gt; has no interface, so handlers depend on the concrete class
    /// directly. Its public methods (FindByEmailAsync, CheckPasswordAsync, etc.) are virtual,
    /// so Moq can still mock it - it just needs a store plus null-able optional dependencies
    /// to satisfy the constructor.
    /// </summary>
    public static class UserManagerMockFactory
    {
        public static Mock<UserManager<ApplicationUser>> Create()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();

            return new Mock<UserManager<ApplicationUser>>(
                store.Object,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!);
        }
    }
}
