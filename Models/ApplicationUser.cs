using Microsoft.AspNetCore.Identity;

namespace YallaEat.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        
        // Navigation properties
        public virtual ICollection<Restaurant> OwnedRestaurants { get; set; } = new List<Restaurant>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
