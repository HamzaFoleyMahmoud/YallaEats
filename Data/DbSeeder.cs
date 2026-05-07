using Microsoft.AspNetCore.Identity;
using YallaEat.Models;

namespace YallaEat.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Create Roles
            string[] roleNames = { "Admin", "Owner", "Customer" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Create Admin User
            string adminEmail = "admin@yallaeats.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    EmailConfirmed = true
                };

                var createAdminResult = await userManager.CreateAsync(newAdmin, "Admin@123");
                if (createAdminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }

            // 3. Seed Dummy Data in Egypt
            if (!context.Restaurants.Any())
            {
                // Create an Owner
                var ownerEmail = "owner@egypt.com";
                var owner = new ApplicationUser
                {
                    UserName = ownerEmail,
                    Email = ownerEmail,
                    FirstName = "Ahmed",
                    LastName = "Hassan",
                    EmailConfirmed = true
                };
                var ownerResult = await userManager.CreateAsync(owner, "Owner@123");
                if (ownerResult.Succeeded) await userManager.AddToRoleAsync(owner, "Owner");

                // Create a Customer
                var customerEmail = "customer@egypt.com";
                var customer = new ApplicationUser
                {
                    UserName = customerEmail,
                    Email = customerEmail,
                    FirstName = "Mona",
                    LastName = "Ali",
                    EmailConfirmed = true
                };
                var customerResult = await userManager.CreateAsync(customer, "Customer@123");
                if (customerResult.Succeeded) await userManager.AddToRoleAsync(customer, "Customer");

                // Add Restaurants
                var restaurants = new List<Restaurant>
                {
                    new Restaurant
                    {
                        Name = "Koshary Abou Tarek",
                        Description = "The most famous and authentic Koshary in downtown Cairo.",
                        City = "Cairo",
                        Address = "Downtown",
                        ImageUrl = "https://images.unsplash.com/photo-1555939594-58d7cb561ad1?q=80&w=2000",
                        OwnerId = owner.Id
                    },
                    new Restaurant
                    {
                        Name = "Seafood by the Sea",
                        Description = "Fresh Alexandrian seafood caught daily.",
                        City = "Alexandria",
                        Address = "Corniche Road",
                        ImageUrl = "https://images.unsplash.com/photo-1579684947550-22e945225d9a?q=80&w=2000",
                        OwnerId = owner.Id
                    },
                    new Restaurant
                    {
                        Name = "Pyramids Grill",
                        Description = "Traditional Egyptian grills and kebabs with a view.",
                        City = "Giza",
                        Address = "Haram Street",
                        ImageUrl = "https://images.unsplash.com/photo-1551782450-a2132b4ba21d?q=80&w=2000",
                        OwnerId = owner.Id
                    }
                };
                context.Restaurants.AddRange(restaurants);
                await context.SaveChangesAsync();

                // Add Menu Items
                var kosharyMenu = new List<MenuItem>
                {
                    new MenuItem { Name = "Classic Koshary", Description = "Rice, macaroni, and lentils topped with spicy tomato sauce.", Price = 40.00m, RestaurantId = restaurants[0].Id },
                    new MenuItem { Name = "Rice Pudding", Description = "Traditional Roz Bel Laban.", Price = 25.00m, RestaurantId = restaurants[0].Id }
                };
                context.MenuItems.AddRange(kosharyMenu);
                await context.SaveChangesAsync();

                // Add Reviews
                var review1 = new Review
                {
                    Rating = 5,
                    Comment = "Absolutely amazing! Best Koshary in Cairo.",
                    UserId = customer.Id,
                    RestaurantId = restaurants[0].Id
                };
                var review2 = new Review
                {
                    Rating = 4,
                    Comment = "Great seafood, wonderful view of the Mediterranean.",
                    UserId = customer.Id,
                    RestaurantId = restaurants[1].Id
                };
                context.Reviews.Add(review1);
                context.Reviews.Add(review2);
                await context.SaveChangesAsync();
            }
            // 4. Seed 30 Random Restaurants
            if (context.Restaurants.Count() < 30)
            {
                var owner = await userManager.FindByEmailAsync("owner@egypt.com");
                if (owner != null)
                {
                    var cities = new[] { "Cairo", "Alexandria", "Giza", "Luxor", "Aswan", "Mansoura", "Suez", "Port Said", "Tanta", "Ismailia" };
                    var foodTypes = new[] { "Koshary", "Seafood", "Grill", "Shawarma", "Pizza", "Burger", "Sushi", "Fried Chicken", "Falafel", "Pasta" };
                    var adjectives = new[] { "Famous", "Tasty", "Royal", "Golden", "Crispy", "Spicy", "Authentic", "Fresh", "Classic", "Premium" };
                    var images = new[] 
                    {
                        "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?w=800",
                        "https://images.unsplash.com/photo-1555396273-367ea4eb4db5?w=800",
                        "https://images.unsplash.com/photo-1552566626-52f8b828add9?w=800",
                        "https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800",
                        "https://images.unsplash.com/photo-1466978913421-bac2e2eca5ab?w=800"
                    };

                    var random = new Random();
                    var newRestaurants = new List<Restaurant>();

                    for (int i = 0; i < 30; i++)
                    {
                        var city = cities[random.Next(cities.Length)];
                        var foodType = foodTypes[random.Next(foodTypes.Length)];
                        var adj = adjectives[random.Next(adjectives.Length)];
                        var img = images[random.Next(images.Length)];

                        newRestaurants.Add(new Restaurant
                        {
                            Name = $"{adj} {foodType} {city}",
                            Description = $"Experience the most {adj.ToLower()} {foodType.ToLower()} in all of {city}. A true culinary delight!",
                            City = city,
                            Address = $"Main Street, {city}",
                            ImageUrl = img,
                            OwnerId = owner.Id
                        });
                    }

                    context.Restaurants.AddRange(newRestaurants);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
