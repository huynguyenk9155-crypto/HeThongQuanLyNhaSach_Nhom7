using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tuan6.Models;

namespace Tuan6.Models
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Seed Roles
            string[] roleNames = { "Admin", "Member" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed Admin User
            var adminEmail = "admin@tuan6.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    FullName = "System Administrator",
                    Address = "Hanoi, Vietnam",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed Member User
            var memberEmail = "member@tuan6.com";
            var memberUser = await userManager.FindByEmailAsync(memberEmail);
            if (memberUser == null)
            {
                memberUser = new ApplicationUser
                {
                    UserName = "member",
                    Email = memberEmail,
                    FullName = "Nguyen Van Member",
                    Address = "Da Nang, Vietnam",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(memberUser, "Member123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(memberUser, "Member");
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(memberUser, "Member"))
                {
                    await userManager.AddToRoleAsync(memberUser, "Member");
                }
            }

            // Seed some initial Categories and Products if none exist
            if (!await context.Categories.AnyAsync())
            {
                var phoneCategory = new Category { Name = "Điện thoại" };
                var laptopCategory = new Category { Name = "Laptop" };

                context.Categories.AddRange(phoneCategory, laptopCategory);
                await context.SaveChangesAsync();

                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "iPhone 15 Pro Max",
                        Price = 30000000,
                        Description = "Điện thoại iPhone 15 Pro Max 256GB chính hãng.",
                        Category = phoneCategory
                    },
                    new Product
                    {
                        Name = "Samsung Galaxy S24 Ultra",
                        Price = 28000000,
                        Description = "Điện thoại Samsung Galaxy S24 Ultra mới nhất.",
                        Category = phoneCategory
                    },
                    new Product
                    {
                        Name = "MacBook Pro M3",
                        Price = 45000000,
                        Description = "Laptop Apple MacBook Pro M3 Pro 18GB 512GB.",
                        Category = laptopCategory
                    },
                    new Product
                    {
                        Name = "Dell XPS 13 Plus",
                        Price = 35000000,
                        Description = "Laptop Dell XPS 13 Plus Core i7 thế hệ mới.",
                        Category = laptopCategory
                    }
                };

                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }
        }
    }
}
