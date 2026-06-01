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

            // Seed initial Categories and Books
            if (!await context.Categories.AnyAsync(c => c.Name == "Văn học"))
            {
                // Clear old categories (e.g. Laptop, Điện thoại) from the database
                var oldCategories = await context.Categories.ToListAsync();
                context.Categories.RemoveRange(oldCategories);
                await context.SaveChangesAsync();

                var literatureCat = new Category { Name = "Văn học" };
                var economyCat = new Category { Name = "Kinh tế" };
                var scienceCat = new Category { Name = "Khoa học" };
                var lifeSkillCat = new Category { Name = "Kỹ năng sống" };

                context.Categories.AddRange(literatureCat, economyCat, scienceCat, lifeSkillCat);
                await context.SaveChangesAsync();

                var books = new List<Book>
                {
                    new Book
                    {
                        Title = "Đắc Nhân Tâm",
                        Author = "Dale Carnegie",
                        Price = 79000,
                        Description = "Cuốn sách đưa ra các lời khuyên về cách thức cư xử, ứng xử và giao tiếp với mọi người để đạt được thành công trong cuộc sống.",
                        ImageUrl = "/images/dac_nhan_tam.jpg",
                        StockQuantity = 150,
                        Category = literatureCat
                    },
                    new Book
                    {
                        Title = "Nhà Giả Kim",
                        Author = "Paulo Coelho",
                        Price = 65000,
                        Description = "Hành trình theo đuổi ước mơ của cậu bé chăn cừu Santiago, truyền cảm hứng mạnh mẽ cho người đọc.",
                        ImageUrl = "/images/nha_gia_kim.jpg",
                        StockQuantity = 120,
                        Category = literatureCat
                    },
                    new Book
                    {
                        Title = "Tư Duy Nhanh Và Chậm",
                        Author = "Daniel Kahneman",
                        Price = 135000,
                        Description = "Kiệt tác phân tích hai hệ thống tư duy chi phối hành vi của con người của nhà tâm lý học đoạt giải Nobel Daniel Kahneman.",
                        ImageUrl = "/images/tu_duy_nhanh_cham.jpg",
                        StockQuantity = 80,
                        Category = scienceCat
                    },
                    new Book
                    {
                        Title = "Sapiens: Lược Sử Loài Người",
                        Author = "Yuval Noah Harari",
                        Price = 108000,
                        Description = "Cuốn sách đi sâu vào lịch sử loài người từ thời kỳ đồ đá cho đến thế kỷ 21, mang đến cái nhìn toàn diện và mới mẻ.",
                        ImageUrl = "/images/sapiens.jpg",
                        StockQuantity = 50,
                        Category = scienceCat
                    },
                    new Book
                    {
                        Title = "Atomic Habits - Thay Đổi Tí Hon, Hiệu Quả Bất Ngờ",
                        Author = "James Clear",
                        Price = 156000,
                        Description = "Phương pháp cực kỳ hiệu quả để xây dựng thói quen tốt và từ bỏ thói quen xấu thông qua những cải tiến nhỏ hàng ngày.",
                        ImageUrl = "/images/atomic_habits.jpg",
                        StockQuantity = 200,
                        Category = lifeSkillCat
                    },
                    new Book
                    {
                        Title = "Dám Bị Ghét",
                        Author = "Ichiro Kishimi",
                        Price = 108000,
                        Description = "Cuốn sách giúp bạn giải phóng bản thân khỏi những kỳ vọng của người khác để sống cuộc đời tự do và hạnh phúc theo triết lý Adler.",
                        ImageUrl = "/images/dam_bi_ghet.jpg",
                        StockQuantity = 95,
                        Category = lifeSkillCat
                    }
                };

                context.Books.AddRange(books);
                await context.SaveChangesAsync();
            }
        }
    }
}
