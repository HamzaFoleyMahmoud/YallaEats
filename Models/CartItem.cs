using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YallaEat.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }

        public int MenuItemId { get; set; }

        [ForeignKey(nameof(MenuItemId))]
        public virtual MenuItem? MenuItem { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
