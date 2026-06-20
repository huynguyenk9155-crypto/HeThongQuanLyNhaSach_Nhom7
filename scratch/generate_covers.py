import os
import sys
import urllib.request
from PIL import Image, ImageDraw, ImageFont

# Reconfigure stdout for UTF-8 to prevent encoding errors on Windows
if sys.stdout.encoding != 'utf-8':
    sys.stdout.reconfigure(encoding='utf-8')

# Define target directory
target_dir = r"c:\Tuan6\wwwroot\images"
os.makedirs(target_dir, exist_ok=True)

# List of 20 new books with details for cover generation
books_data = [
    {
        "filename": "SoDo.jpg",
        "title": "SỐ ĐỎ",
        "author": "Vũ Trọng Phụng",
        "category": "Văn học",
        "colors": [(200, 30, 30), (80, 10, 10)] # Crimson Gradient
    },
    {
        "filename": "RungNaUy.jpg",
        "title": "RỪNG NA UY",
        "author": "Haruki Murakami",
        "category": "Văn học",
        "colors": [(20, 60, 40), (10, 30, 20)] # Forest Green Gradient
    },
    {
        "filename": "ChaGiauChaNgheo.jpg",
        "title": "CHA GIÀU\nCHA NGHÈO",
        "author": "Robert Kiyosaki",
        "category": "Kinh tế",
        "colors": [(40, 60, 150), (10, 20, 60)] # Dark Blue Gradient
    },
    {
        "filename": "QuocGiaKhoiNghiep.jpg",
        "title": "QUỐC GIA\nKHỞI NGHIỆP",
        "author": "Dan Senor & Saul Singer",
        "category": "Kinh tế",
        "colors": [(0, 120, 180), (0, 40, 80)] # Ocean Blue
    },
    {
        "filename": "VuTru.jpg",
        "title": "VŨ TRỤ",
        "author": "Carl Sagan",
        "category": "Khoa học",
        "colors": [(10, 10, 40), (40, 20, 90)] # Space Violet
    },
    {
        "filename": "LuocSuThoiGian.jpg",
        "title": "LƯỢC SỬ\nTHỜI GIAN",
        "author": "Stephen Hawking",
        "category": "Khoa học",
        "colors": [(30, 30, 60), (10, 10, 20)] # Cosmic Black
    },
    {
        "filename": "DoiThayDoiKhiChungTaThayDoi.jpg",
        "title": "ĐỜI THAY ĐỔI\nKHI CHÚNG TA\nTHAY ĐỔI",
        "author": "Andrew Matthews",
        "category": "Kỹ năng sống",
        "colors": [(220, 100, 20), (120, 50, 10)] # Sunset Orange
    },
    {
        "filename": "KheoAnNoiSeCoDuocThienHa.jpg",
        "title": "KHÉO ĂN NÓI\nSẼ CÓ ĐƯỢC\nTHIÊN HẠ",
        "author": "Trác Nhã",
        "category": "Kỹ năng sống",
        "colors": [(200, 80, 120), (100, 30, 60)] # Plum Gradient
    },
    {
        "filename": "SuViet12KhucTrangCa.jpg",
        "title": "SỬ VIỆT\n12 KHÚC TRÁNG CA",
        "author": "Dũng Phan",
        "category": "Lịch sử",
        "colors": [(160, 110, 40), (80, 50, 10)] # Golden Brown
    },
    {
        "filename": "BaoTapTrieuTran.jpg",
        "title": "BÃO TÁP\nTRIỀU TRẦN",
        "author": "Hoàng Quốc Hải",
        "category": "Lịch sử",
        "colors": [(120, 20, 20), (50, 5, 5)] # Royal Maroon
    },
    {
        "filename": "KinhVanHoa.jpg",
        "title": "KÍNH VẠN HOA",
        "author": "Nguyễn Nhật Ánh",
        "category": "Thiếu nhi",
        "colors": [(30, 160, 120), (10, 80, 60)] # Teal Gradient
    },
    {
        "filename": "KhongGiaDinh.jpg",
        "title": "KHÔNG GIA ĐÌNH",
        "author": "Hector Malot",
        "category": "Thiếu nhi",
        "colors": [(100, 150, 200), (40, 70, 110)] # Sky Blue
    },
    {
        "filename": "HackNao1500Tu.jpg",
        "title": "HACK NÃO\n1500 TỪ TIẾNG ANH",
        "author": "Nguyễn Văn Hiệp",
        "category": "Ngoại ngữ",
        "colors": [(210, 130, 20), (100, 60, 5)] # Amber
    },
    {
        "filename": "LuyenSieuTriNhoTuVung.jpg",
        "title": "LUYỆN SIÊU TRÍ NHỚ\nTỪ VỰNG TIẾNG ANH",
        "author": "Nguyễn Anh Đức",
        "category": "Ngoại ngữ",
        "colors": [(40, 150, 80), (10, 70, 30)] # Green Grass
    },
    {
        "filename": "HieuVeTraiTim.jpg",
        "title": "HIỂU VỀ\nTRÁI TIM",
        "author": "Minh Niệm",
        "category": "Tâm lý học",
        "colors": [(180, 50, 90), (90, 10, 40)] # Rose/Heart Red
    },
    {
        "filename": "NgheThuatTuDuyRanhMach.jpg",
        "title": "NGHỆ THUẬT\nTƯ DUY RÀNH MẠCH",
        "author": "Rolf Dobelli",
        "category": "Tâm lý học",
        "colors": [(80, 80, 90), (30, 30, 35)] # Slate Gray
    },
    {
        "filename": "PragmaticProgrammer.jpg",
        "title": "THE PRAGMATIC\nPROGRAMMER",
        "author": "Andrew Hunt & D. Thomas",
        "category": "Công nghệ",
        "colors": [(40, 50, 70), (15, 20, 30)] # Dark Steel
    },
    {
        "filename": "DesigningDataIntensive.jpg",
        "title": "DESIGNING\nDATA-INTENSIVE\nAPPLICATIONS",
        "author": "Martin Kleppmann",
        "category": "Công nghệ",
        "colors": [(20, 100, 150), (5, 40, 70)] # Tech Cyan
    },
    {
        "filename": "YTuongNayLaCuaChungMinh.jpg",
        "title": "Ý TƯỞNG NÀY\nLÀ CỦA CHÚNG MÌNH",
        "author": "Huỳnh Vĩnh Sơn",
        "category": "Nghệ thuật",
        "colors": [(180, 60, 180), (70, 10, 70)] # Vibrant Purple
    },
    {
        "filename": "HieuVeNgheThuat.jpg",
        "title": "HIỂU VỀ\nNGHỆ THUẬT",
        "author": "Nhiều tác giả",
        "category": "Nghệ thuật",
        "colors": [(150, 100, 50), (70, 45, 20)] # Clay/Artistic Brown
    }
]

# Try to find a nice system font
font_title_path = "C:\\Windows\\Fonts\\georgiab.ttf" # Georgia Bold
font_author_path = "C:\\Windows\\Fonts\\georgia.ttf" # Georgia Regular
font_cat_path = "C:\\Windows\\Fonts\\arial.ttf" # Arial Regular

if not os.path.exists(font_title_path):
    font_title_path = "arial.ttf" # Fallback to standard
    font_author_path = "arial.ttf"
    font_cat_path = "arial.ttf"

for book in books_data:
    filepath = os.path.join(target_dir, book["filename"])
    print(f"Generating cover for: {book['title']} -> {filepath}")
    
    # Create image (300 x 450)
    width, height = 300, 450
    image = Image.new("RGB", (width, height))
    draw = ImageDraw.Draw(image)
    
    # Draw vertical gradient
    color_start, color_end = book["colors"]
    for y in range(height):
        # Linear interpolation
        r = int(color_start[0] + (color_end[0] - color_start[0]) * (y / height))
        g = int(color_start[1] + (color_end[1] - color_start[1]) * (y / height))
        b = int(color_start[2] + (color_end[2] - color_start[2]) * (y / height))
        draw.line([(0, y), (width, y)], fill=(r, g, b))
        
    # Draw decorative borders/frame
    draw.rectangle([15, 15, width - 16, height - 16], outline=(255, 255, 255, 60), width=1)
    draw.rectangle([20, 20, width - 21, height - 21], outline=(255, 255, 255, 120), width=1)
    
    # Load fonts
    try:
        font_title = ImageFont.truetype(font_title_path, 22)
        font_author = ImageFont.truetype(font_author_path, 14)
        font_cat = ImageFont.truetype(font_cat_path, 11)
    except IOError:
        font_title = ImageFont.load_default()
        font_author = ImageFont.load_default()
        font_cat = ImageFont.load_default()
        
    # Draw Category badge at the top
    cat_text = book["category"].upper()
    # Estimate size
    cat_w = draw.textlength(cat_text, font=font_cat) if hasattr(draw, "textlength") else len(cat_text) * 6
    draw.rectangle([(width - cat_w) // 2 - 10, 35, (width + cat_w) // 2 + 10, 55], fill=(255, 255, 255, 40))
    draw.text((width // 2, 45), cat_text, fill=(255, 255, 255), font=font_cat, anchor="mm")
    
    # Draw Title (centered vertically/horizontally)
    title_text = book["title"]
    # Split title lines
    lines = title_text.split("\n")
    y_text = height // 2 - 30
    for line in lines:
        draw.text((width // 2, y_text), line, fill=(255, 255, 255), font=font_title, anchor="mm")
        y_text += 30
        
    # Draw Author at bottom
    author_text = book["author"]
    draw.text((width // 2, height - 60), author_text, fill=(240, 240, 240), font=font_author, anchor="mm")
    
    # Draw decorative logo or emblem at center-bottom
    draw.ellipse([width // 2 - 15, height - 110, width // 2 + 15, height - 80], outline=(255, 255, 255, 100), width=1)
    draw.text((width // 2, height - 95), "B", fill=(255, 255, 255, 150), font=font_cat, anchor="mm")
    
    # Save Image
    image.save(filepath, "JPEG", quality=95)

print("All 20 covers generated successfully!")
