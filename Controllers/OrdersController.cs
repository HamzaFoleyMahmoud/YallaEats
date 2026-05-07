using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YallaEat.Data;
using YallaEat.Models;

namespace YallaEat.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .ThenInclude(m => m.Restaurant)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            
            // Allow user to see their own order, or Admin/Owner of a restaurant in the order
            if (order.UserId != userId && !User.IsInRole("Admin"))
            {
                // check if user is owner of any restaurant in this order
                var restaurantIds = order.OrderItems.Select(oi => oi.MenuItem.RestaurantId).Distinct();
                var userRestaurants = await _context.Restaurants
                    .Where(r => r.OwnerId == userId)
                    .Select(r => r.Id)
                    .ToListAsync();

                if (!restaurantIds.Any(id => userRestaurants.Contains(id)))
                {
                    return Forbid();
                }
            }

            return View(order);
        }
    }
}
