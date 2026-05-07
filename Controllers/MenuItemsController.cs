using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YallaEat.Data;
using YallaEat.Models;

namespace YallaEat.Controllers
{
    [Authorize(Roles = "Owner,Admin")]
    public class MenuItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MenuItemsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<bool> IsOwnerOrAdmin(int restaurantId)
        {
            if (User.IsInRole("Admin")) return true;
            var user = await _userManager.GetUserAsync(User);
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);
            return restaurant != null && restaurant.OwnerId == user?.Id;
        }

        // GET: MenuItems/Create?restaurantId=5
        public async Task<IActionResult> Create(int restaurantId)
        {
            if (!await IsOwnerOrAdmin(restaurantId)) return Forbid();
            ViewBag.RestaurantId = restaurantId;
            return View();
        }

        // POST: MenuItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Price,ImageUrl,RestaurantId")] MenuItem menuItem)
        {
            if (!await IsOwnerOrAdmin(menuItem.RestaurantId)) return Forbid();
            
            if (ModelState.IsValid)
            {
                _context.Add(menuItem);
                await _context.SaveChangesAsync();
                return RedirectToAction("ManageMenu", "Restaurants", new { id = menuItem.RestaurantId });
            }
            return View(menuItem);
        }

        // GET: MenuItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null) return NotFound();

            if (!await IsOwnerOrAdmin(menuItem.RestaurantId)) return Forbid();

            return View(menuItem);
        }

        // POST: MenuItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,ImageUrl,RestaurantId")] MenuItem menuItem)
        {
            if (id != menuItem.Id) return NotFound();
            if (!await IsOwnerOrAdmin(menuItem.RestaurantId)) return Forbid();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(menuItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MenuItemExists(menuItem.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction("ManageMenu", "Restaurants", new { id = menuItem.RestaurantId });
            }
            return View(menuItem);
        }

        // POST: MenuItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null) return NotFound();
            
            if (!await IsOwnerOrAdmin(menuItem.RestaurantId)) return Forbid();

            var restaurantId = menuItem.RestaurantId;
            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("ManageMenu", "Restaurants", new { id = restaurantId });
        }

        private bool MenuItemExists(int id)
        {
            return _context.MenuItems.Any(e => e.Id == id);
        }
    }
}
