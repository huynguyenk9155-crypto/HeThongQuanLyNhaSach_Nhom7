using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tuan6.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;

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

            // Copy images from Images folder to wwwroot/images
            var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            var sourceImagesDir = Path.Combine(env.ContentRootPath, "Images");
            var targetImagesDir = Path.Combine(env.WebRootPath, "images");

            if (Directory.Exists(sourceImagesDir))
            {
                if (!Directory.Exists(targetImagesDir))
                {
                    Directory.CreateDirectory(targetImagesDir);
                }

                foreach (var file in Directory.GetFiles(sourceImagesDir))
                {
                    var destFile = Path.Combine(targetImagesDir, Path.GetFileName(file));
                    File.Copy(file, destFile, true);
                }
            }

            // Update existing books' ImageUrls if they contain old naming convention
            var imageMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "/images/dac_nhan_tam.jpg", "/images/DacNhanTam.jpg" },
                { "/images/nha_gia_kim.jpg", "/images/NhaGiaKim.jpg" },
                { "/images/tu_duy_nhanh_cham.jpg", "/images/TuDuyNhanhCham.jpg" },
                { "/images/sapiens.jpg", "/images/Sapiens.jpg" },
                { "/images/atomic_habits.jpg", "/images/atomic.jpg" },
                { "/images/dam_bi_ghet.jpg", "/images/DamBiGhet.jpg" }
            };

            var existingBooks = await context.Books.ToListAsync();
            bool modified = false;
            foreach (var book in existingBooks)
            {
                if (book.ImageUrl != null && imageMap.TryGetValue(book.ImageUrl, out var newUrl))
                {
                    book.ImageUrl = newUrl;
                    modified = true;
                }
            }
            if (modified)
            {
                await context.SaveChangesAsync();
            }

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
            if (await context.Categories.CountAsync() < 10)
            {
                var existingCategories = await context.Categories.ToListAsync();
                var categoryNames = new[] { "Văn học", "Kinh tế", "Khoa học", "Kỹ năng sống", "Lịch sử", "Thiếu nhi", "Ngoại ngữ", "Tâm lý học", "Công nghệ", "Nghệ thuật" };
                
                foreach (var name in categoryNames)
                {
                    if (!existingCategories.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    {
                        var newCat = new Category { Name = name };
                        context.Categories.Add(newCat);
                        existingCategories.Add(newCat);
                    }
                }
                await context.SaveChangesAsync();

                var literatureCat = existingCategories.First(c => c.Name.Equals("Văn học", StringComparison.OrdinalIgnoreCase));
                var economyCat = existingCategories.First(c => c.Name.Equals("Kinh tế", StringComparison.OrdinalIgnoreCase));
                var scienceCat = existingCategories.First(c => c.Name.Equals("Khoa học", StringComparison.OrdinalIgnoreCase));
                var lifeSkillCat = existingCategories.First(c => c.Name.Equals("Kỹ năng sống", StringComparison.OrdinalIgnoreCase));
                var historyCat = existingCategories.First(c => c.Name.Equals("Lịch sử", StringComparison.OrdinalIgnoreCase));
                var childrenCat = existingCategories.First(c => c.Name.Equals("Thiếu nhi", StringComparison.OrdinalIgnoreCase));
                var languageCat = existingCategories.First(c => c.Name.Equals("Ngoại ngữ", StringComparison.OrdinalIgnoreCase));
                var psychologyCat = existingCategories.First(c => c.Name.Equals("Tâm lý học", StringComparison.OrdinalIgnoreCase));
                var techCat = existingCategories.First(c => c.Name.Equals("Công nghệ", StringComparison.OrdinalIgnoreCase));
                var artCat = existingCategories.First(c => c.Name.Equals("Nghệ thuật", StringComparison.OrdinalIgnoreCase));

                var books = new List<Book>
                {
                    new Book
                    {
                        Title = "Đắc Nhân Tâm",
                        Author = "Dale Carnegie",
                        Price = 79000,
                        Description = "Cuốn sách đưa ra các lời khuyên về cách thức cư xử, ứng xử và giao tiếp với mọi người để đạt được thành công trong cuộc sống.",
                        ImageUrl = "/images/DacNhanTam.jpg",
                        StockQuantity = 150,
                        Category = literatureCat
                    },
                    new Book
                    {
                        Title = "Nhà Giả Kim",
                        Author = "Paulo Coelho",
                        Price = 65000,
                        Description = "Hành trình theo đuổi ước mơ của cậu bé chăn cừu Santiago, truyền cảm hứng mạnh mẽ cho người đọc.",
                        ImageUrl = "/images/NhaGiaKim.jpg",
                        StockQuantity = 120,
                        Category = literatureCat
                    },
                    new Book
                    {
                        Title = "Tư Duy Nhanh Và Chậm",
                        Author = "Daniel Kahneman",
                        Price = 135000,
                        Description = "Kiệt tác phân tích hai hệ thống tư duy chi phối hành vi của con người của nhà tâm lý học đoạt giải Nobel Daniel Kahneman.",
                        ImageUrl = "/images/TuDuyNhanhCham.jpg",
                        StockQuantity = 80,
                        Category = scienceCat
                    },
                    new Book
                    {
                        Title = "Sapiens: Lược Sử Loài Người",
                        Author = "Yuval Noah Harari",
                        Price = 108000,
                        Description = "Cuốn sách đi sâu vào lịch sử loài người từ thời kỳ đồ đá cho đến thế kỷ 21, mang đến cái nhìn toàn diện và mới mẻ.",
                        ImageUrl = "/images/Sapiens.jpg",
                        StockQuantity = 50,
                        Category = scienceCat
                    },
                    new Book
                    {
                        Title = "Atomic Habits - Thay Đổi Tí Hon, Hiệu Quả Bất Ngờ",
                        Author = "James Clear",
                        Price = 156000,
                        Description = "Phương pháp cực kỳ hiệu quả để xây dựng thói quen tốt và từ bỏ thói quen xấu thông qua những cải tiến nhỏ hàng ngày.",
                        ImageUrl = "/images/atomic.jpg",
                        StockQuantity = 200,
                        Category = lifeSkillCat
                    },
                    new Book
                    {
                        Title = "Dám Bị Ghét",
                        Author = "Ichiro Kishimi",
                        Price = 108000,
                        Description = "Cuốn sách giúp bạn giải phóng bản thân khỏi những kỳ vọng của người khác để sống cuộc đời tự do và hạnh phúc theo triết lý Adler.",
                        ImageUrl = "/images/DamBiGhet.jpg",
                        StockQuantity = 95,
                        Category = lifeSkillCat
                    },
                    new Book
                    {
                        Title = "Lược Sử Việt Nam",
                        Author = "Trần Trọng Kim",
                        Price = 85000,
                        Description = "Tác phẩm tóm lược toàn bộ tiến trình lịch sử dựng nước và giữ nước của dân tộc Việt Nam qua các thời kỳ.",
                        ImageUrl = "",
                        StockQuantity = 50,
                        Category = historyCat
                    },
                    new Book
                    {
                        Title = "Dế Mèn Phiêu Lưu Ký",
                        Author = "Tô Hoài",
                        Price = 45000,
                        Description = "Tác phẩm văn học thiếu nhi kinh điển của nhà văn Tô Hoài kể về những cuộc phiêu lưu đầy thú vị của chú Dế Mèn.",
                        ImageUrl = "",
                        StockQuantity = 150,
                        Category = childrenCat
                    },
                    new Book
                    {
                        Title = "English Grammar in Use",
                        Author = "Raymond Murphy",
                        Price = 125000,
                        Description = "Cuốn sách ngữ pháp tiếng Anh bán chạy nhất thế giới dành cho người học trình độ trung cấp.",
                        ImageUrl = "",
                        StockQuantity = 90,
                        Category = languageCat
                    },
                    new Book
                    {
                        Title = "Tâm Lý Học Đám Đông",
                        Author = "Gustave Le Bon",
                        Price = 95000,
                        Description = "Tác phẩm kinh điển phân tích về tâm lý, hành vi và các quy luật chi phối đám đông trong xã hội.",
                        ImageUrl = "",
                        StockQuantity = 60,
                        Category = psychologyCat
                    },
                    new Book
                    {
                        Title = "Clean Code",
                        Author = "Robert C. Martin",
                        Price = 250000,
                        Description = "Cuốn sách cẩm nang kinh điển giúp các lập trình viên viết mã nguồn sạch, dễ đọc và dễ bảo trì.",
                        ImageUrl = "",
                        StockQuantity = 40,
                        Category = techCat
                    },
                    new Book
                    {
                        Title = "Câu Chuyện Nghệ Thuật",
                        Author = "E.H. Gombrich",
                        Price = 350000,
                        Description = "Một trong những cuốn sách nhập môn lịch sử nghệ thuật nổi tiếng và có sức ảnh hưởng nhất mọi thời đại.",
                        ImageUrl = "",
                        StockQuantity = 15,
                        Category = artCat
                    }
                };

                var existingBooksList = await context.Books.ToListAsync();
                foreach (var b in books)
                {
                    if (!existingBooksList.Any(eb => eb.Title.Equals(b.Title, StringComparison.OrdinalIgnoreCase)))
                    {
                        context.Books.Add(b);
                    }
                }
                await context.SaveChangesAsync();
            }
        }
    }
}
