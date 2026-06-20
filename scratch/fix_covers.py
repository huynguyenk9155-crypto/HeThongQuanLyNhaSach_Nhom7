import os
from PIL import Image, ImageDraw, ImageFont

def generate_beautiful_cover(filepath, title, author, color_start, color_end):
    # Generates a beautiful gradient cover with Arial Bold font (supports Vietnamese perfectly)
    width, height = 400, 600
    image = Image.new("RGB", (width, height))
    draw = ImageDraw.Draw(image)
    
    # Draw gradient
    for y in range(height):
        r = int(color_start[0] + (color_end[0] - color_start[0]) * (y / height))
        g = int(color_start[1] + (color_end[1] - color_start[1]) * (y / height))
        b = int(color_start[2] + (color_end[2] - color_start[2]) * (y / height))
        draw.line([(0, y), (width, y)], fill=(r, g, b))
        
    # Frames
    draw.rectangle([20, 20, width - 21, height - 21], outline=(255, 255, 255, 40), width=1)
    draw.rectangle([28, 28, width - 29, height - 29], outline=(255, 255, 255, 120), width=3)
    
    # Fonts: Arialbd.ttf is guaranteed on Windows
    font_path = r"C:\Windows\Fonts\arialbd.ttf"
    try:
        font_title = ImageFont.truetype(font_path, 28)
        font_author = ImageFont.truetype(font_path, 18)
        font_logo = ImageFont.truetype(font_path, 14)
    except IOError:
        font_title = ImageFont.load_default()
        font_author = ImageFont.load_default()
        font_logo = ImageFont.load_default()
        
    # Title (wrap text)
    words = title.split()
    lines = []
    current_line = []
    for word in words:
        current_line.append(word)
        test_line = " ".join(current_line)
        w = draw.textlength(test_line, font=font_title) if hasattr(draw, "textlength") else len(test_line) * 14
        if w > 320:
            current_line.pop()
            lines.append(" ".join(current_line))
            current_line = [word]
    if current_line:
        lines.append(" ".join(current_line))
        
    y_text = height // 2 - (len(lines) * 20)
    for line in lines:
        draw.text((width // 2, y_text), line, fill=(255, 255, 255), font=font_title, anchor="mm")
        y_text += 40
        
    # Author
    draw.text((width // 2, height - 80), author, fill=(240, 240, 240), font=font_author, anchor="mm")
    
    # Emblem
    draw.ellipse([width // 2 - 16, height - 150, width // 2 + 16, height - 118], outline=(255, 255, 255, 120), width=2)
    draw.text((width // 2, height - 134), "B", fill=(255, 255, 255, 180), font=font_logo, anchor="mm")
    
    try:
        image.save(filepath, "JPEG", quality=95)
        print("Generated cover successfully")
    except Exception as e:
        print(f"Error saving image: {e}")

# Paths
target_dir1 = r"c:\Tuan6\wwwroot\images"
target_dir2 = r"c:\Tuan6\Images"
os.makedirs(target_dir2, exist_ok=True)

# 1. Khéo Ăn Nói Sẽ Có Được Thiên Hạ - Warm red-orange gradient
for target_dir in [target_dir1, target_dir2]:
    generate_beautiful_cover(
        os.path.join(target_dir, "KheoAnNoiSeCoDuocThienHa.jpg"),
        "KHÉO ĂN NÓI SẼ CÓ ĐƯỢC THIÊN HẠ",
        "Trác Nhã",
        (180, 40, 40), # Crimson Red
        (60, 10, 10)   # Dark Crimson
    )

# 2. Đời Thay Đổi Khi Chúng Ta Thay Đổi - Optimistic Teal gradient
for target_dir in [target_dir1, target_dir2]:
    generate_beautiful_cover(
        os.path.join(target_dir, "DoiThayDoiKhiChungTaThayDoi.jpg"),
        "ĐỜI THAY ĐỔI KHI CHÚNG TA THAY ĐỔI",
        "Andrew Matthews",
        (0, 150, 136), # Teal
        (0, 38, 53)    # Deep Navy/Teal
    )
