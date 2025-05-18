using Microsoft.AspNetCore.Identity;

namespace vuez.Models
{
    public class ApplicationUser : IdentityUser
    {
        // tvoje vlastné vlastnosti
        public Guid? RoleId { get; set; }
        public Role? Role { get; set; }
        public UserDetails? Details { get; set; }
    }
}
