using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YallaEat.Data;
using YallaEat.Models;
using YallaEat.Models.ViewModels;

namespace YallaEat.Controllers
{
    [Authorize(Roles = "Owner,Admin")]
    public class RestaurantsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RestaurantsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> SalesAnalytics(int? id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var query = _context.OrderItems
                .Include(oi => oi.MenuItem)
                .ThenInclude(m => m.Restaurant)
                .Where(oi => oi.MenuItem.Restaurant.OwnerId == user.Id);

            if (id.HasValue)
            {
                query = query.Where(oi => oi.MenuItem.RestaurantId == id.Value);
                var restaurant = await _context.Restaurants.FindAsync(id.Value);
                ViewBag.RestaurantName = restaurant?.Name;
            }

            var salesData = await query
                .GroupBy(oi => new { oi.MenuItem.Name, oi.MenuItem.ImageUrl })
                .Select(g => new SalesPerformanceViewModel
                {
                    ItemName = g.Key.Name,
                    ImageUrl = g.Key.ImageUrl,
                    TotalSold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.PriceAtTimeOfOrder)
                })
                .OrderByDescending(s => s.TotalSold)
                .ToListAsync();

            return View(salesData);
        }

        // GET: Restaurants/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var restaurant = await _context.Restaurants
                .Include(r => r.MenuItems)
                .Include(r => r.Reviews)
                .ThenInclude(rev => rev.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (restaurant == null) return NotFound();

            return View(restaurant);
        }

        // GET: Restaurants/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var restaurants = await _context.Restaurants
                .Where(r => r.OwnerId == user.Id)
                .ToListAsync();

            return View(restaurants);
        }

        // GET: Restaurants/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (restaurant.OwnerId != user?.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(restaurant);
        }

        // POST: Restaurants/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,City,Address,ImageUrl,OwnerId")] Restaurant restaurant)
        {
            if (id != restaurant.Id) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (restaurant.OwnerId != user?.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(restaurant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RestaurantExists(restaurant.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(restaurant);
        }

        private bool RestaurantExists(int id)
        {
            return _context.Restaurants.Any(e => e.Id == id);
        }

        // GET: Restaurants/ManageMenu/5
        public async Task<IActionResult> ManageMenu(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var restaurant = await _context.Restaurants
                .Include(r => r.MenuItems)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (restaurant == null) return NotFound();

            // Security check: Only owner or admin
            if (restaurant.OwnerId != user.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(restaurant);
        }

        // GET: Restaurants/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Restaurants/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,City,Address,ImageUrl")] Restaurant restaurant)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            restaurant.OwnerId = user.Id;

            if (ModelState.IsValid)
            {
                _context.Add(restaurant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(restaurant);
        }

        // POST: Restaurants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var restaurant = await _context.Restaurants.FindAsync(id);
            
            // Ensure only the owner (or admin) can delete
            if (restaurant != null && (restaurant.OwnerId == user.Id || User.IsInRole("Admin")))
            {
                _context.Restaurants.Remove(restaurant);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Restaurants/Orders
        public async Task<IActionResult> Orders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .ThenInclude(m => m.Restaurant)
                .Where(o => o.OrderItems.Any(oi => oi.MenuItem.Restaurant.OwnerId == user.Id))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .ThenInclude(m => m.Restaurant)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return NotFound();

            // Security check: Does the user own at least one restaurant in this order?
            if (!User.IsInRole("Admin") && !order.OrderItems.Any(oi => oi.MenuItem.Restaurant.OwnerId == user.Id))
            {
                return Forbid();
            }

            order.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Orders));
        }
    }
}
