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

                // Seed initial Authors if empty
                if (!await context.Authors.AnyAsync())
                {
                    context.Authors.AddRange(new List<Author>
                    {
                        new Author { Name = "Dale Carnegie", Biography = "Dale Carnegie là một nhà văn và nhà thuyết trình người Mỹ." },
                        new Author { Name = "Paulo Coelho", Biography = "Paulo Coelho là tiểu thuyết gia nổi tiếng người Brazil." },
                        new Author { Name = "Daniel Kahneman", Biography = "Daniel Kahneman là nhà tâm lý học người Mỹ gốc Israel đoạt giải Nobel." },
                        new Author { Name = "Yuval Noah Harari", Biography = "Yuval Noah Harari là một nhà nghiên cứu lịch sử người Israel." },
                        new Author { Name = "James Clear", Biography = "James Clear là một tác giả và diễn giả chuyên về thói quen." },
                        new Author { Name = "Ichiro Kishimi", Biography = "Ichiro Kishimi là nhà triết học và tâm lý học người Nhật Bản." },
                        new Author { Name = "Trần Trọng Kim", Biography = "Trần Trọng Kim là một nhà giáo dục, nhà biên khảo lịch sử Việt Nam." },
                        new Author { Name = "Tô Hoài", Biography = "Tô Hoài là một nhà văn lớn của văn học Việt Nam hiện đại." },
                        new Author { Name = "Raymond Murphy", Biography = "Raymond Murphy là tác giả nổi tiếng của nhiều sách học tiếng Anh." },
                        new Author { Name = "Gustave Le Bon", Biography = "Gustave Le Bon là một nhà tâm lý học xã hội người Pháp." },
                        new Author { Name = "Robert C. Martin", Biography = "Robert C. Martin (Uncle Bob) là một kỹ sư phần mềm nổi tiếng." },
                        new Author { Name = "E.H. Gombrich", Biography = "Ernst Hans Josef Gombrich là nhà sử học nghệ thuật người Anh gốc Áo." }
                    });
                    await context.SaveChangesAsync();
                }

                // Seed initial Publishers if empty
                if (!await context.Publishers.AnyAsync())
                {
                    context.Publishers.AddRange(new List<Publisher>
                    {
                        new Publisher { Name = "NXB Trẻ", Address = "161B Lý Chính Thắng, Quận 3, TP.HCM", Email = "hopthu@nxbtre.com.vn" },
                        new Publisher { Name = "NXB Kim Đồng", Address = "55 Quang Trung, Hai Bà Trưng, Hà Nội", Email = "info@nxbkimdong.com.vn" },
                        new Publisher { Name = "NXB Thế Giới", Address = "46 Trần Hưng Đạo, Hà Nội", Email = "thegioi@hn.vnn.vn" },
                        new Publisher { Name = "NXB Lao Động", Address = "175 Giảng Võ, Hà Nội", Email = "nxblaodong@yahoo.com" },
                        new Publisher { Name = "NXB Hội Nhà Văn", Address = "65 Nguyễn Du, Hà Nội", Email = "nxbhoinhavan@gmail.com" },
                        new Publisher { Name = "NXB Tổng hợp TP.HCM", Address = "86 Nguyễn Thị Minh Khai, Quận 1, TP.HCM", Email = "nxb@nxbhcm.com.vn" }
                    });
                    await context.SaveChangesAsync();
                }

                var authorList = await context.Authors.ToListAsync();
                var publisherList = await context.Publishers.ToListAsync();

                var getAuthorId = new Func<string, int?>(name => authorList.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Id);
                var getPublisherId = new Func<string, int?>(name => publisherList.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Id);

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
                        Category = literatureCat,
                        AuthorId = getAuthorId("Dale Carnegie"),
                        PublisherId = getPublisherId("NXB Trẻ")
                    },
                    new Book
                    {
                        Title = "Nhà Giả Kim",
                        Author = "Paulo Coelho",
                        Price = 65000,
                        Description = "Hành trình theo đuổi ước mơ của cậu bé chăn cừu Santiago, truyền cảm hứng mạnh mẽ cho người đọc.",
                        ImageUrl = "/images/NhaGiaKim.jpg",
                        StockQuantity = 120,
                        Category = literatureCat,
                        AuthorId = getAuthorId("Paulo Coelho"),
                        PublisherId = getPublisherId("NXB Hội Nhà Văn")
                    },
                    new Book
                    {
                        Title = "Tư Duy Nhanh Và Chậm",
                        Author = "Daniel Kahneman",
                        Price = 135000,
                        Description = "Kiệt tác phân tích hai hệ thống tư duy chi phối hành vi của con người của nhà tâm lý học đoạt giải Nobel Daniel Kahneman.",
                        ImageUrl = "/images/TuDuyNhanhCham.jpg",
                        StockQuantity = 80,
                        Category = scienceCat,
                        AuthorId = getAuthorId("Daniel Kahneman"),
                        PublisherId = getPublisherId("NXB Thế Giới")
                    },
                    new Book
                    {
                        Title = "Sapiens: Lược Sử Loài Người",
                        Author = "Yuval Noah Harari",
                        Price = 108000,
                        Description = "Cuốn sách đi sâu vào lịch sử loài người từ thời kỳ đồ đá cho đến thế kỷ 21, mang đến cái nhìn toàn diện và mới mẻ.",
                        ImageUrl = "/images/Sapiens.jpg",
                        StockQuantity = 50,
                        Category = scienceCat,
                        AuthorId = getAuthorId("Yuval Noah Harari"),
                        PublisherId = getPublisherId("NXB Thế Giới")
                    },
                    new Book
                    {
                        Title = "Atomic Habits - Thay Đổi Tí Hon, Hiệu Quả Bất Ngờ",
                        Author = "James Clear",
                        Price = 156000,
                        Description = "Phương pháp cực kỳ hiệu quả để xây dựng thói quen tốt và từ bỏ thói quen xấu thông qua những cải tiến nhỏ hàng ngày.",
                        ImageUrl = "/images/atomic.jpg",
                        StockQuantity = 200,
                        Category = lifeSkillCat,
                        AuthorId = getAuthorId("James Clear"),
                        PublisherId = getPublisherId("NXB Lao Động")
                    },
                    new Book
                    {
                        Title = "Dám Bị Ghét",
                        Author = "Ichiro Kishimi",
                        Price = 108000,
                        Description = "Cuốn sách giúp bạn giải phóng bản thân khỏi những kỳ vọng của người khác để sống cuộc đời tự do và hạnh phúc theo triết lý Adler.",
                        ImageUrl = "/images/DamBiGhet.jpg",
                        StockQuantity = 95,
                        Category = lifeSkillCat,
                        AuthorId = getAuthorId("Ichiro Kishimi"),
                        PublisherId = getPublisherId("NXB Lao Động")
                    },
                    new Book
                    {
                        Title = "Lược Sử Việt Nam",
                        Author = "Trần Trọng Kim",
                        Price = 85000,
                        Description = "Tác phẩm tóm lược toàn bộ tiến trình lịch sử dựng nước và giữ nước của dân tộc Việt Nam qua các thời kỳ.",
                        ImageUrl = "",
                        StockQuantity = 50,
                        Category = historyCat,
                        AuthorId = getAuthorId("Trần Trọng Kim"),
                        PublisherId = getPublisherId("NXB Tổng hợp TP.HCM")
                    },
                    new Book
                    {
                        Title = "Dế Mèn Phiêu Lưu Ký",
                        Author = "Tô Hoài",
                        Price = 45000,
                        Description = "Tác phẩm văn học thiếu nhi kinh điển của nhà văn Tô Hoài kể về những cuộc phiêu lưu đầy thú vị của chú Dế Mèn.",
                        ImageUrl = "",
                        StockQuantity = 150,
                        Category = childrenCat,
                        AuthorId = getAuthorId("Tô Hoài"),
                        PublisherId = getPublisherId("NXB Kim Đồng")
                    },
                    new Book
                    {
                        Title = "English Grammar in Use",
                        Author = "Raymond Murphy",
                        Price = 125000,
                        Description = "Cuốn sách ngữ pháp tiếng Anh bán chạy nhất thế giới dành cho người học trình độ trung cấp.",
                        ImageUrl = "",
                        StockQuantity = 90,
                        Category = languageCat,
                        AuthorId = getAuthorId("Raymond Murphy"),
                        PublisherId = getPublisherId("NXB Trẻ")
                    },
                    new Book
                    {
                        Title = "Tâm Lý Học Đám Đông",
                        Author = "Gustave Le Bon",
                        Price = 95000,
                        Description = "Tác phẩm kinh điển phân tích về tâm lý, hành vi và các quy luật chi phối đám đông trong xã hội.",
                        ImageUrl = "",
                        StockQuantity = 60,
                        Category = psychologyCat,
                        AuthorId = getAuthorId("Gustave Le Bon"),
                        PublisherId = getPublisherId("NXB Thế Giới")
                    },
                    new Book
                    {
                        Title = "Clean Code",
                        Author = "Robert C. Martin",
                        Price = 250000,
                        Description = "Cuốn sách cẩm nang kinh điển giúp các lập trình viên viết mã nguồn sạch, dễ đọc và dễ bảo trì.",
                        ImageUrl = "",
                        StockQuantity = 40,
                        Category = techCat,
                        AuthorId = getAuthorId("Robert C. Martin"),
                        PublisherId = getPublisherId("NXB Lao Động")
                    },
                    new Book
                    {
                        Title = "Câu Chuyện Nghệ Thuật",
                        Author = "E.H. Gombrich",
                        Price = 350000,
                        Description = "Một trong những cuốn sách nhập môn lịch sử nghệ thuật nổi tiếng và có sức ảnh hưởng nhất mọi thời đại.",
                        ImageUrl = "",
                        StockQuantity = 15,
                        Category = artCat,
                        AuthorId = getAuthorId("E.H. Gombrich"),
                        PublisherId = getPublisherId("NXB Thế Giới")
                    }
                };

                var existingBooksList = await context.Books.ToListAsync();
                foreach (var b in books)
                {
                    var existingBook = existingBooksList.FirstOrDefault(eb => eb.Title.Equals(b.Title, StringComparison.OrdinalIgnoreCase));
                    if (existingBook == null)
                    {
                        context.Books.Add(b);
                    }
                    else
                    {
                        // Update AuthorId and PublisherId for existing books if not set
                        if (existingBook.AuthorId == null) existingBook.AuthorId = b.AuthorId;
                        if (existingBook.PublisherId == null) existingBook.PublisherId = b.PublisherId;
                    }
                }
                await context.SaveChangesAsync();
            }
        }
    }
}
