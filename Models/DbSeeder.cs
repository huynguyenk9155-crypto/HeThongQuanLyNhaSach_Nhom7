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

            // Hard delete the 6 books with mismatched covers, along with their order details to prevent FK constraint violations
            var titlesToDelete = new[] { "Lược Sử Việt Nam", "Dế Mèn Phiêu Lưu Ký", "English Grammar in Use", "Tâm Lý Học Đám Đông", "Clean Code", "Câu Chuyện Nghệ Thuật" };
            foreach (var title in titlesToDelete)
            {
                var bookToDelete = await context.Books.FirstOrDefaultAsync(b => b.Title == title);
                if (bookToDelete != null)
                {
                    var orderDetails = await context.OrderDetails.Where(od => od.BookId == bookToDelete.Id).ToListAsync();
                    if (orderDetails.Any())
                    {
                        context.OrderDetails.RemoveRange(orderDetails);
                    }
                    context.Books.Remove(bookToDelete);
                }
            }
            await context.SaveChangesAsync();

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
            var existingCategories = await context.Categories.ToListAsync();
            var categoryNames = new[] { "Văn học", "Kinh tế", "Khoa học", "Kỹ năng sống", "Lịch sử", "Thiếu nhi", "Ngoại ngữ", "Tâm lý học", "Công nghệ", "Nghệ thuật", "Dụng cụ học tập" };
            
            bool categoriesModified = false;
            foreach (var name in categoryNames)
            {
                if (!existingCategories.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    var newCat = new Category { Name = name };
                    context.Categories.Add(newCat);
                    existingCategories.Add(newCat);
                    categoriesModified = true;
                }
            }
            if (categoriesModified)
            {
                await context.SaveChangesAsync();
            }

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
            var schoolSuppliesCat = existingCategories.First(c => c.Name.Equals("Dụng cụ học tập", StringComparison.OrdinalIgnoreCase));

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
                    Title = "Số Đỏ",
                    Author = "Vũ Trọng Phụng",
                    Price = 68000,
                    Description = "Tác phẩm văn học hiện thực xuất sắc của Vũ Trọng Phụng châm biếm xã hội thượng lưu nửa mùa.",
                    ImageUrl = "/images/SoDo.jpg",
                    StockQuantity = 100,
                    Category = literatureCat
                },
                new Book
                {
                    Title = "Rừng Na Uy",
                    Author = "Haruki Murakami",
                    Price = 128000,
                    Description = "Tiểu thuyết nổi tiếng nhất của Haruki Murakami, một câu chuyện tình yêu u buồn và sâu sắc của tuổi trẻ.",
                    ImageUrl = "/images/RungNaUy.jpg",
                    StockQuantity = 90,
                    Category = literatureCat,
                    PublisherId = getPublisherId("NXB Hội Nhà Văn")
                },
                new Book
                {
                    Title = "Cha Giàu Cha Nghèo",
                    Author = "Robert Kiyosaki",
                    Price = 98000,
                    Description = "Cuốn sách dạy con làm giàu kinh điển giúp bạn thay đổi tư duy về tài chính và tiền bạc.",
                    ImageUrl = "/images/ChaGiauChaNgheo.jpg",
                    StockQuantity = 110,
                    Category = economyCat,
                    PublisherId = getPublisherId("NXB Trẻ")
                },
                new Book
                {
                    Title = "Quốc Gia Khởi Nghiệp",
                    Author = "Dan Senor & Saul Singer",
                    Price = 118000,
                    Description = "Câu chuyện về sự phát triển thần kỳ của nền kinh tế Israel từ một quốc gia nhỏ bé thành trung tâm công nghệ hàng đầu.",
                    ImageUrl = "/images/QuocGiaKhoiNghiep.jpg",
                    StockQuantity = 75,
                    Category = economyCat,
                    PublisherId = getPublisherId("NXB Thế Giới")
                },
                new Book
                {
                    Title = "Vũ Trụ",
                    Author = "Carl Sagan",
                    Price = 165000,
                    Description = "Tác phẩm khoa học đại chúng kinh điển đưa người đọc khám phá những bí ẩn kỳ thú của vũ trụ bao la.",
                    ImageUrl = "/images/VuTru.jpg",
                    StockQuantity = 60,
                    Category = scienceCat,
                    PublisherId = getPublisherId("NXB Thế Giới")
                },
                new Book
                {
                    Title = "Lược Sử Thời Gian",
                    Author = "Stephen Hawking",
                    Price = 95000,
                    Description = "Tác phẩm giải thích các khái niệm vật lý thiên văn phức tạp như hố đen, thời gian và sự hình thành vũ trụ một cách dễ hiểu.",
                    ImageUrl = "/images/LuocSuThoiGian.jpg",
                    StockQuantity = 85,
                    Category = scienceCat,
                    PublisherId = getPublisherId("NXB Thế Giới")
                },
                new Book
                {
                    Title = "Đời Thay Đổi Khi Chúng Ta Thay Đổi",
                    Author = "Andrew Matthews",
                    Price = 75000,
                    Description = "Cuốn sách giúp bạn thay đổi thái độ sống để đón nhận hạnh phúc và vượt qua những khó khăn thường nhật.",
                    ImageUrl = "/images/DoiThayDoiKhiChungTaThayDoi.jpg",
                    StockQuantity = 140,
                    Category = lifeSkillCat,
                    PublisherId = getPublisherId("NXB Trẻ")
                },
                new Book
                {
                    Title = "Khéo Ăn Nói Sẽ Có Được Thiên Hạ",
                    Author = "Trác Nhã",
                    Price = 85000,
                    Description = "Phương pháp rèn luyện kỹ năng giao tiếp khéo léo để đạt được thành công trong công việc và cuộc sống.",
                    ImageUrl = "/images/KheoAnNoiSeCoDuocThienHa.jpg",
                    StockQuantity = 160,
                    Category = lifeSkillCat
                },
                new Book
                {
                    Title = "Sử Việt - 12 Khúc Tráng Ca",
                    Author = "Dũng Phan",
                    Price = 99000,
                    Description = "Tác phẩm lịch sử kể lại 12 giai đoạn hào hùng nhất của dân tộc Việt Nam với giọng văn cuốn hút và đầy tự hào.",
                    ImageUrl = "/images/SuViet12KhucTrangCa.jpg",
                    StockQuantity = 95,
                    Category = historyCat,
                    PublisherId = getPublisherId("NXB Hội Nhà Văn")
                },
                new Book
                {
                    Title = "Bão Táp Triều Trần",
                    Author = "Hoàng Quốc Hải",
                    Price = 195000,
                    Description = "Tiểu thuyết lịch sử tái hiện hoành tráng triều đại nhà Trần và những cuộc kháng chiến chống quân Nguyên Mông.",
                    ImageUrl = "/images/BaoTapTrieuTran.jpg",
                    StockQuantity = 45,
                    Category = historyCat
                },
                new Book
                {
                    Title = "Kính Vạn Hoa",
                    Author = "Nguyễn Nhật Ánh",
                    Price = 125000,
                    Description = "Bộ truyện học trò vui nhộn kể về ba bạn nhỏ Quý ròm, Tiểu Long và Hạnh với những cuộc phiêu lưu lý thú.",
                    ImageUrl = "/images/KinhVanHoa.jpg",
                    StockQuantity = 180,
                    Category = childrenCat,
                    PublisherId = getPublisherId("NXB Kim Đồng")
                },
                new Book
                {
                    Title = "Không Gia Đình",
                    Author = "Hector Malot",
                    Price = 98000,
                    Description = "Hành trình gian khổ nhưng đầy nghị lực và tình yêu thương của cậu bé Remi mồ côi đi lang thang khắp nước Pháp.",
                    ImageUrl = "/images/KhongGiaDinh.jpg",
                    StockQuantity = 130,
                    Category = childrenCat,
                    PublisherId = getPublisherId("NXB Kim Đồng")
                },
                new Book
                {
                    Title = "Hack Não 1500 Từ Tiếng Anh",
                    Author = "Nguyễn Văn Hiệp",
                    Price = 395000,
                    Description = "Sách học từ vựng tiếng Anh đột phá với phương pháp âm thanh tương tự và truyện chêm sinh động.",
                    ImageUrl = "/images/HackNao1500Tu.jpg",
                    StockQuantity = 220,
                    Category = languageCat,
                    PublisherId = getPublisherId("NXB Lao Động")
                },
                new Book
                {
                    Title = "Luyện Siêu Trí Nhớ Từ Vựng Tiếng Anh",
                    Author = "Nguyễn Anh Đức",
                    Price = 145000,
                    Description = "Bí quyết giúp người học nâng cao vốn từ vựng tiếng Anh nhanh chóng thông qua sơ đồ tư duy.",
                    ImageUrl = "/images/LuyenSieuTriNhoTuVung.jpg",
                    StockQuantity = 110,
                    Category = languageCat
                },
                new Book
                {
                    Title = "Hiểu Về Trái Tim",
                    Author = "Minh Niệm",
                    Price = 120000,
                    Description = "Cuốn sách giúp người đọc nhìn sâu vào tâm hồn, hóa giải những nỗi đau và tìm lại sự bình yên trong cuộc sống.",
                    ImageUrl = "/images/HieuVeTraiTim.jpg",
                    StockQuantity = 125,
                    Category = psychologyCat,
                    PublisherId = getPublisherId("NXB Trẻ")
                },
                new Book
                {
                    Title = "Nghệ Thuật Tư Duy Rành Mạch",
                    Author = "Rolf Dobelli",
                    Price = 115000,
                    Description = "Chỉ ra 99 lỗi tư duy thường gặp để giúp bạn đưa ra những quyết định sáng suốt và chính xác hơn.",
                    ImageUrl = "/images/NgheThuatTuDuyRanhMach.jpg",
                    StockQuantity = 70,
                    Category = psychologyCat,
                    PublisherId = getPublisherId("NXB Thế Giới")
                },
                new Book
                {
                    Title = "The Pragmatic Programmer",
                    Author = "Andrew Hunt & D. Thomas",
                    Price = 280000,
                    Description = "Cuốn sách cẩm nang kinh điển dành cho lập trình viên để nâng cao kỹ năng viết mã và làm việc nhóm hiệu quả.",
                    ImageUrl = "/images/PragmaticProgrammer.jpg",
                    StockQuantity = 35,
                    Category = techCat,
                    PublisherId = getPublisherId("NXB Lao Động")
                },
                new Book
                {
                    Title = "Designing Data-Intensive Applications",
                    Author = "Martin Kleppmann",
                    Price = 320000,
                    Description = "Sách chuyên sâu về thiết kế hệ thống dữ liệu lớn, phân tán và có khả năng chịu tải cao cực kỳ nổi tiếng.",
                    ImageUrl = "/images/DesigningDataIntensive.jpg",
                    StockQuantity = 25,
                    Category = techCat,
                    PublisherId = getPublisherId("NXB Lao Động")
                },
                new Book
                {
                    Title = "Ý Tưởng Này Là Của Chúng Mình",
                    Author = "Huỳnh Vĩnh Sơn",
                    Price = 88000,
                    Description = "Cuốn sách hài hước, chân thực chia sẻ về thế giới của những người làm sáng tạo và viết quảng cáo (copywriter).",
                    ImageUrl = "/images/YTuongNayLaCuaChungMinh.jpg",
                    StockQuantity = 90,
                    Category = artCat,
                    PublisherId = getPublisherId("NXB Trẻ")
                },
                new Book
                {
                    Title = "Hiểu Về Nghệ Thuật",
                    Author = "Nhiều tác giả",
                    Price = 290000,
                    Description = "Cuốn sách giúp bạn tiếp cận nghệ thuật hội họa và lịch sử mỹ thuật thế giới qua các thời kỳ một cách sinh động.",
                    ImageUrl = "/images/HieuVeNgheThuat.jpg",
                    StockQuantity = 30,
                    Category = artCat,
                    PublisherId = getPublisherId("NXB Thế Giới")
                },
                new Book
                {
                    Title = "Máy tính Casio fx-580VN X",
                    Author = "Casio",
                    Price = 650000,
                    Description = "Máy tính khoa học thế hệ mới hỗ trợ đắc lực cho học sinh, sinh viên giải các bài toán phức tạp.",
                    ImageUrl = "/images/CasioCalculator.png",
                    StockQuantity = 80,
                    Category = schoolSuppliesCat
                },
                new Book
                {
                    Title = "Bút chì màu hộp gỗ Colokit",
                    Author = "Thiên Long",
                    Price = 120000,
                    Description = "Bộ 24 màu sắc tươi sáng, mịn màng, an toàn cho trẻ em tự do vẽ tranh.",
                    ImageUrl = "/images/ColoredPencils.png",
                    StockQuantity = 150,
                    Category = schoolSuppliesCat
                },
                new Book
                {
                    Title = "Bút bi Thiên Long FO-03 (Hộp 20 cây)",
                    Author = "Thiên Long",
                    Price = 75000,
                    Description = "Bút viết êm trơn, mực ra đều, thiết kế nhỏ gọn tiện lợi.",
                    ImageUrl = "/images/BallpointPen.png",
                    StockQuantity = 200,
                    Category = schoolSuppliesCat
                },
                new Book
                {
                    Title = "Thước kẻ gỗ học sinh 20cm",
                    Author = "Bến Nghé",
                    Price = 15000,
                    Description = "Thước kẻ làm từ gỗ tự nhiên chất lượng cao, vạch chia rõ ràng, độ bền tốt.",
                    ImageUrl = "/images/WoodenRuler.png",
                    StockQuantity = 120,
                    Category = schoolSuppliesCat
                },
                new Book
                {
                    Title = "Sổ tay lò xo A5 bìa da",
                    Author = "Klong",
                    Price = 45000,
                    Description = "Cuốn sổ ghi chép tiện dụng với giấy chống lóa mắt, bìa da sang trọng.",
                    ImageUrl = "/images/NotebookSpiral.png",
                    StockQuantity = 100,
                    Category = schoolSuppliesCat
                }
            };

            var existingBooksList = await context.Books.ToListAsync();
            bool booksModified = false;
            foreach (var b in books)
            {
                var existingBook = existingBooksList.FirstOrDefault(eb => eb.Title.Equals(b.Title, StringComparison.OrdinalIgnoreCase));
                if (existingBook == null)
                {
                    context.Books.Add(b);
                    booksModified = true;
                }
                else
                {
                    // Force update ImageUrl, CategoryId, AuthorId, PublisherId if they are different from our clean seed data
                    if (existingBook.ImageUrl != b.ImageUrl || existingBook.CategoryId != b.Category?.Id || existingBook.AuthorId != b.AuthorId || existingBook.PublisherId != b.PublisherId)
                    {
                        existingBook.ImageUrl = b.ImageUrl;
                        if (b.Category != null)
                        {
                            existingBook.CategoryId = b.Category.Id;
                        }
                        if (b.AuthorId != null)
                        {
                            existingBook.AuthorId = b.AuthorId;
                        }
                        if (b.PublisherId != null)
                        {
                            existingBook.PublisherId = b.PublisherId;
                        }
                        context.Books.Update(existingBook);
                        booksModified = true;
                    }
                }
            }
            if (booksModified)
            {
                await context.SaveChangesAsync();
                existingBooksList = await context.Books.ToListAsync();
            }

            // Delete any books from database that are not in the seed list (to remove books with unverified/incorrect covers), excluding those with orders
            var seedTitles = books.Select(b => b.Title.ToLower()).ToList();
            var booksToDelete = new List<Book>();
            foreach (var eb in existingBooksList)
            {
                if (!seedTitles.Contains(eb.Title.ToLower()))
                {
                    bool hasOrders = await context.OrderDetails.AnyAsync(od => od.BookId == eb.Id);
                    if (!hasOrders)
                    {
                        booksToDelete.Add(eb);
                    }
                }
            }

            if (booksToDelete.Any())
            {
                context.Books.RemoveRange(booksToDelete);
                await context.SaveChangesAsync();
            }
        }
    }
}
