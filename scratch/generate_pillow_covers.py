import os
from PIL import Image, ImageDraw, ImageFont

# Define book data
books_data = [
    {
        "filename": "DacNhanTam.jpg",
        "title": "ĐẮC NHÂN TÂM",
        "author": "Dale Carnegie",
        "category": "KỸ NĂNG SỐNG",
        "bg_color": "#800020", # Burgundy
        "font_type": "serif"
    },
    {
        "filename": "NhaGiaKim.jpg",
        "title": "NHÀ GIẢ KIM",
        "author": "Paulo Coelho",
        "category": "TIỂU THUYẾT",
        "bg_color": "#D97706", # Warm Amber
        "font_type": "serif"
    },
    {
        "filename": "TuDuyNhanhCham.jpg",
        "title": "TƯ DUY NHANH VÀ CHẬM",
        "author": "Daniel Kahneman",
        "category": "KHOA HỌC TÂM LÝ",
        "bg_color": "#0F766E", # Teal
        "font_type": "sans"
    },
    {
        "filename": "Sapiens.jpg",
        "title": "SAPIENS\nLƯỢC SỬ LOÀI NGƯỜI",
        "author": "Yuval Noah Harari",
        "category": "KHOA HỌC LỊCH SỬ",
        "bg_color": "#C2410C", # Clay
        "font_type": "sans"
    },
    {
        "filename": "atomic.jpg",
        "title": "ATOMIC HABITS\nThay Đổi Tí Hon\nHiệu Quả Bất Ngờ",
        "author": "James Clear",
        "category": "KỸ NĂNG PHÁT TRIỂN",
        "bg_color": "#1E3A8A", # Deep Blue
        "font_type": "sans"
    },
    {
        "filename": "DamBiGhet.jpg",
        "title": "DÁM BỊ GHÉT",
        "author": "Ichiro Kishimi",
        "category": "TÂM LÝ HỌC ADLER",
        "bg_color": "#1E3F20", # Sage Green
        "font_type": "serif"
    },
    {
        "filename": "SoDo.jpg",
        "title": "SỐ ĐỎ",
        "author": "Vũ Trọng Phụng",
        "category": "VĂN HỌC VIỆT NAM",
        "bg_color": "#DC2626", # Vivid Red
        "font_type": "serif"
    },
    {
        "filename": "RungNaUy.jpg",
        "title": "RỪNG NA UY",
        "author": "Haruki Murakami",
        "category": "TIỂU THUYẾT",
        "bg_color": "#065F46", # Forest Green
        "font_type": "serif"
    },
    {
        "filename": "ChaGiauChaNgheo.jpg",
        "title": "CHA GIÀU CHA NGHÈO",
        "author": "Robert Kiyosaki",
        "category": "KINH TẾ - TÀI CHÍNH",
        "bg_color": "#064E3B", # Dark Emerald
        "font_type": "sans"
    },
    {
        "filename": "QuocGiaKhoiNghiep.jpg",
        "title": "QUỐC GIA KHỞI NGHIỆP",
        "author": "Dan Senor & Saul Singer",
        "category": "KINH TẾ - KHỞI NGHIỆP",
        "bg_color": "#1D4ED8", # Royal Blue
        "font_type": "sans"
    },
    {
        "filename": "VuTru.jpg",
        "title": "VŨ TRỤ",
        "author": "Carl Sagan",
        "category": "KHOA HỌC VŨ TRỤ",
        "bg_color": "#4C1D95", # Midnight Purple
        "font_type": "serif"
    },
    {
        "filename": "LuocSuThoiGian.jpg",
        "title": "LƯỢC SỬ THỜI GIAN",
        "author": "Stephen Hawking",
        "category": "VẬT LÝ THIÊN VĂN",
        "bg_color": "#1F2937", # Charcoal
        "font_type": "sans"
    },
    {
        "filename": "DoiThayDoiKhiChungTaThayDoi.jpg",
        "title": "ĐỜI THAY ĐỔI\nKhi Chúng Ta Thay Đổi",
        "author": "Andrew Matthews",
        "category": "KỸ NĂNG SỐNG",
        "bg_color": "#EA580C", # Orange
        "font_type": "sans"
    },
    {
        "filename": "KheoAnNoiSeCoDuocThienHa.jpg",
        "title": "KHÉO ĂN NÓI\nSẽ Có Được Thiên Hạ",
        "author": "Trác Nhã",
        "category": "KỸ NĂNG GIAO TIẾP",
        "bg_color": "#701A75", # Plum
        "font_type": "serif"
    },
    {
        "filename": "SuViet12KhucTrangCa.jpg",
        "title": "SỬ VIỆT\n12 Khúc Tráng Ca",
        "author": "Dũng Phan",
        "category": "LỊCH SỬ VIỆT NAM",
        "bg_color": "#7C2D12", # Dark Rust
        "font_type": "serif"
    },
    {
        "filename": "BaoTapTrieuTran.jpg",
        "title": "BÃO TÁP TRIỀU TRẦN",
        "author": "Hoàng Quốc Hải",
        "category": "TIỂU THUYẾT LỊCH SỬ",
        "bg_color": "#9A3412", # Dark Maroon
        "font_type": "serif"
    },
    {
        "filename": "KinhVanHoa.jpg",
        "title": "KÍNH VẠN HOA",
        "author": "Nguyễn Nhật Ánh",
        "category": "TRUYỆN THIẾU NHI",
        "bg_color": "#0D9488", # Teal Green
        "font_type": "serif"
    },
    {
        "filename": "KhongGiaDinh.jpg",
        "title": "KHÔNG GIA ĐÌNH",
        "author": "Hector Malot",
        "category": "VĂN HỌC NƯỚC NGOÀI",
        "bg_color": "#1E1B4B", # Navy
        "font_type": "serif"
    },
    {
        "filename": "HackNao1500Tu.jpg",
        "title": "HACK NÃO\n1500 Từ Tiếng Anh",
        "author": "Nguyễn Văn Hiệp",
        "category": "HỌC TIẾNG ANH",
        "bg_color": "#0891B2", # Cyan
        "font_type": "sans"
    },
    {
        "filename": "LuyenSieuTriNhoTuVung.jpg",
        "title": "LUYỆN SIÊU TRÍ NHỚ\nTừ Vựng Tiếng Anh",
        "author": "Nguyễn Anh Đức",
        "category": "HỌC TIẾNG ANH",
        "bg_color": "#2563EB", # Blue
        "font_type": "sans"
    },
    {
        "filename": "HieuVeTraiTim.jpg",
        "title": "HIỂU VỀ TRÁI TIM",
        "author": "Minh Niệm",
        "category": "TÂM LÝ - THIỀN - TRỊ LIỆU",
        "bg_color": "#9D174D", # Rose
        "font_type": "serif"
    },
    {
        "filename": "NgheThuatTuDuyRanhMach.jpg",
        "title": "NGHỆ THUẬT\nTƯ DUY RÀNH MẠCH",
        "author": "Rolf Dobelli",
        "category": "TƯ DUY LOGIC",
        "bg_color": "#0F172A", # Charcoal
        "font_type": "sans"
    },
    {
        "filename": "PragmaticProgrammer.jpg",
        "title": "THE PRAGMATIC\nPROGRAMMER",
        "author": "Andrew Hunt & D. Thomas",
        "category": "KỸ THUẬT LẬP TRÌNH",
        "bg_color": "#1E293B", # Slate
        "font_type": "sans"
    },
    {
        "filename": "DesigningDataIntensive.jpg",
        "title": "Designing Data-Intensive\nApplications",
        "author": "Martin Kleppmann",
        "category": "HỆ THỐNG DỮ LIỆU",
        "bg_color": "#374151", # Dark Slate
        "font_type": "sans"
    },
    {
        "filename": "YTuongNayLaCuaChungMinh.jpg",
        "title": "Ý TƯỞNG NÀY LÀ\nCỦA CHÚNG MÌNH",
        "author": "Huỳnh Vĩnh Sơn",
        "category": "SÁNG TẠO - QUẢNG CÁO",
        "bg_color": "#E11D48", # Pinkish Red
        "font_type": "serif"
    },
    {
        "filename": "HieuVeNgheThuat.jpg",
        "title": "HIỂU VỀ NGHỆ THUẬT",
        "author": "Nhiều tác giả",
        "category": "LỊCH SỬ MỸ THUẬT",
        "bg_color": "#5B21B6", # Violet
        "font_type": "serif"
    }
]

# Set up paths
images_dir = "c:\\Tuan6\\Images"
wwwroot_images_dir = "c:\\Tuan6\\wwwroot\\images"

os.makedirs(images_dir, exist_ok=True)
os.makedirs(wwwroot_images_dir, exist_ok=True)

# Select system fonts based on Windows paths
serif_font_path = "C:\\Windows\\Fonts\\timesbd.ttf"  # Times New Roman Bold
serif_regular_path = "C:\\Windows\\Fonts\\times.ttf"  # Times New Roman Regular
sans_font_path = "C:\\Windows\\Fonts\\arialbd.ttf"    # Arial Bold
sans_regular_path = "C:\\Windows\\Fonts\\arial.ttf"    # Arial Regular

width, height = 400, 580

for book in books_data:
    # Create image
    img = Image.new("RGB", (width, height), color=book["bg_color"])
    draw = ImageDraw.Draw(img)
    
    # Load fonts
    if book["font_type"] == "serif":
        title_font = ImageFont.truetype(serif_font_path, 28)
        author_font = ImageFont.truetype(serif_regular_path, 18)
        category_font = ImageFont.truetype(serif_regular_path, 14)
    else:
        title_font = ImageFont.truetype(sans_font_path, 26)
        author_font = ImageFont.truetype(sans_regular_path, 18)
        category_font = ImageFont.truetype(sans_regular_path, 13)

    # Draw ornamental frame
    border_margin = 15
    draw.rectangle(
        [(border_margin, border_margin), (width - border_margin, height - border_margin)],
        outline="#FFFFFF",
        width=2
    )
    
    # Inner border line
    inner_margin = border_margin + 5
    draw.rectangle(
        [(inner_margin, inner_margin), (width - inner_margin, height - inner_margin)],
        outline="#FFFFFF",
        width=1
    )

    # 1. Draw Category Tag
    tag_y = border_margin + 35
    category_text = book["category"]
    # Get text width
    cat_bbox = draw.textbbox((0, 0), category_text, font=category_font)
    cat_w = cat_bbox[2] - cat_bbox[0]
    draw.text(((width - cat_w) / 2, tag_y), category_text, font=category_font, fill="#F3F4F6")

    # Draw divider below tag
    divider_y = tag_y + 25
    draw.line(((width/2) - 40, divider_y, (width/2) + 40, divider_y), fill="#E5E7EB", width=1)

    # 2. Draw Title (centered in the upper middle)
    title_text = book["title"]
    lines = title_text.split('\n')
    
    # Calculate starting Y to center title block vertically in the main area
    title_start_y = height / 2.3 - (len(lines) * 18)
    
    for i, line in enumerate(lines):
        line_bbox = draw.textbbox((0, 0), line, font=title_font)
        line_w = line_bbox[2] - line_bbox[0]
        line_h = line_bbox[3] - line_bbox[1]
        draw.text(((width - line_w) / 2, title_start_y + (i * 38)), line, font=title_font, fill="#FFFFFF")

    # Draw small divider
    author_divider_y = height - 120
    draw.line(((width/2) - 30, author_divider_y, (width/2) + 30, author_divider_y), fill="#E5E7EB", width=1)

    # 3. Draw Author
    author_text = book["author"]
    author_bbox = draw.textbbox((0, 0), author_text, font=author_font)
    author_w = author_bbox[2] - author_bbox[0]
    draw.text(((width - author_w) / 2, author_divider_y + 15), author_text, font=author_font, fill="#F3F4F6")

    # 4. Draw small brand signature
    draw.text(((width - 70) / 2, height - border_margin - 30), "BOOKSTORE", font=category_font, fill="#9CA3AF")

    # Save to both folders
    dest1 = os.path.join(images_dir, book["filename"])
    dest2 = os.path.join(wwwroot_images_dir, book["filename"])
    
    img.save(dest1, "JPEG", quality=95)
    img.save(dest2, "JPEG", quality=95)
    print(f"Generated clean cover for: {book['filename']}")

print("All offline covers generated successfully!")
