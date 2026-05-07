using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YallaEat.Data;
using YallaEat.Models;

namespace YallaEat.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> ManageRestaurants()
        {
            var restaurants = await _context.Restaurants.Include(r => r.Owner).ToListAsync();
            return View(restaurants);
        }

        public async Task<IActionResult> AllOrders()
        {
            var orders = await _context.Orders.Include(o => o.User).OrderByDescending(o => o.OrderDate).ToListAsync();
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> MakeOwner(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Remove from Customer if exists, add to Owner
            await _userManager.RemoveFromRoleAsync(user, "Customer");
            if (!await _userManager.IsInRoleAsync(user, "Owner"))
            {
                await _userManager.AddToRoleAsync(user, "Owner");
            }

            return RedirectToAction(nameof(ManageUsers));
        }
    }
}
