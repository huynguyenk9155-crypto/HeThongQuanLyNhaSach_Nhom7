import os
import sys
import re
import urllib.parse
import requests
from PIL import Image, ImageDraw, ImageFont

# Reconfigure stdout for UTF-8 to prevent encoding errors on Windows
if sys.stdout.encoding != 'utf-8':
    sys.stdout.reconfigure(encoding='utf-8')

target_dir = r"c:\Tuan6\wwwroot\images"
os.makedirs(target_dir, exist_ok=True)

books_search = [
    {"filename": "SoDo.jpg", "query": "So Do Vu Trong Phung bia sach", "title": "SỐ ĐỎ", "author": "Vũ Trọng Phụng"},
    {"filename": "RungNaUy.jpg", "query": "Rung Na Uy Haruki Murakami bia sach", "title": "RỪNG NA UY", "author": "Haruki Murakami"},
    {"filename": "ChaGiauChaNgheo.jpg", "query": "Cha Giau Cha Ngheo Robert Kiyosaki bia sach", "title": "CHA GIÀU CHA NGHÈO", "author": "Robert Kiyosaki"},
    {"filename": "QuocGiaKhoiNghiep.jpg", "query": "Quoc Gia Khoi Nghiep Dan Senor bia sach", "title": "QUỐC GIA KHỞI NGHIỆP", "author": "Dan Senor & Saul Singer"},
    {"filename": "VuTru.jpg", "query": "Vu Tru Carl Sagan bia sach", "title": "VŨ TRỤ", "author": "Carl Sagan"},
    {"filename": "LuocSuThoiGian.jpg", "query": "Luoc Su Thoi Gian Stephen Hawking bia sach", "title": "LƯỢC SỬ THỜI GIAN", "author": "Stephen Hawking"},
    {"filename": "DoiThayDoiKhiChungTaThayDoi.jpg", "query": "Doi Thay Doi Khi Chung Ta Thay Doi Andrew Matthews bia sach", "title": "ĐỜI THAY ĐỔI KHI CHÚNG TA THAY ĐỔI", "author": "Andrew Matthews"},
    {"filename": "KheoAnNoiSeCoDuocThienHa.jpg", "query": "Kheo An Noi Se Co Duoc Thien Ha Trac Nha bia sach", "title": "KHÉO ĂN NÓI SẼ CÓ ĐƯỢC THIÊN HẠ", "author": "Trác Nhã"},
    {"filename": "SuViet12KhucTrangCa.jpg", "query": "Su Viet 12 Khuc Trang Ca Dung Phan bia sach", "title": "SỬ VIỆT - 12 KHÚC TRÁNG CA", "author": "Dũng Phan"},
    {"filename": "BaoTapTrieuTran.jpg", "query": "Bao Tap Trieu Tran Hoang Quoc Hai bia sach", "title": "BÃO TÁP TRIỀU TRẦN", "author": "Hoàng Quốc Hải"},
    {"filename": "KinhVanHoa.jpg", "query": "Kinh Van Hoa Nguyen Nhat Anh bia sach", "title": "KÍNH VẠN HOA", "author": "Nguyễn Nhật Ánh"},
    {"filename": "KhongGiaDinh.jpg", "query": "Khong Gia Dinh Hector Malot bia sach", "title": "KHÔNG GIA ĐÌNH", "author": "Hector Malot"},
    {"filename": "HackNao1500Tu.jpg", "query": "Hack Nao 1500 Tu Tieng Anh bia sach", "title": "HACK NÃO 1500 TỪ TIẾNG ANH", "author": "Nguyễn Văn Hiệp"},
    {"filename": "LuyenSieuTriNhoTuVung.jpg", "query": "Luyen Sieu Tri Nho Tu Vung Tieng Anh bia sach", "title": "LUYỆN SIÊU TRÍ NHỚ TỪ VỰNG TIẾNG ANH", "author": "Nguyễn Anh Đức"},
    {"filename": "HieuVeTraiTim.jpg", "query": "Hieu Ve Trai Tim Minh Niem bia sach", "title": "HIỂU VỀ TRÁI TIM", "author": "Minh Niệm"},
    {"filename": "NgheThuatTuDuyRanhMach.jpg", "query": "Nghe Thuat Tu Duy Ranh Mach Rolf Dobelli bia sach", "title": "NGHỆ THUẬT TƯ DUY RÀNH MẠCH", "author": "Rolf Dobelli"},
    {"filename": "PragmaticProgrammer.jpg", "query": "The Pragmatic Programmer book cover", "title": "THE PRAGMATIC PROGRAMMER", "author": "Andrew Hunt & D. Thomas"},
    {"filename": "DesigningDataIntensive.jpg", "query": "Designing Data Intensive Applications book cover", "title": "DESIGNING DATA-INTENSIVE APPLICATIONS", "author": "Martin Kleppmann"},
    {"filename": "YTuongNayLaCuaChungMinh.jpg", "query": "Y Tuong Nay Le Cua Chung Minh Huynh Vinh Son bia sach", "title": "Ý TƯỞNG NÀY LÀ CỦA CHÚNG MÌNH", "author": "Huỳnh Vĩnh Sơn"},
    {"filename": "HieuVeNgheThuat.jpg", "query": "Hieu Ve Nghe Thuat bia sach", "title": "HIỂU VỀ NGHỆ THUẬT", "author": "Nhiều tác giả"}
]

WHITELIST_DOMAINS = [
    'tikicdn.com', 'shopee.vn', 'lazcdn.com', 'fahasa.com', 'wikimedia.org',
    'goodreads.com', 'gr-assets.com', 'nhanam.com.vn', 'nxbkimdong.com.vn', 
    'nxbtre.com.vn', 'vinabook.com', 'alphabooks.vn', 'ybook.vn', 'nxbkimdong.com',
    'nxbkimdong.com.vn', 'nxbtre.com', 'nxbtre.com.vn', 'tiki.vn'
]

# Blacklist adult/porn/unrelated content domains to be absolutely safe
BLACKLIST_DOMAINS = [
    'erodoujinlog.com', 'eporner.com', 'javstore.net', 'doujin-assets.dmm.co.jp',
    'dmm.co.jp', 'xnxx', 'pornhub', 'xvideos', 'rule34', 'gelbooru', 'danbooru',
    'yande.re', 'sankakucomplex'
]

def is_safe_and_trusted(url):
    url_lower = url.lower()
    # Check if any blacklisted domain is in the URL
    if any(b_domain in url_lower for b_domain in BLACKLIST_DOMAINS):
        return False
    # Check if a whitelisted domain is in the URL
    return any(w_domain in url_lower for w_domain in WHITELIST_DOMAINS)

def generate_beautiful_cover(filepath, title, author):
    # Generates a beautiful gradient cover with Arial Bold font (supports Vietnamese perfectly)
    width, height = 300, 450
    image = Image.new("RGB", (width, height))
    draw = ImageDraw.Draw(image)
    
    # Gradient colors
    color_start = (30, 40, 90) # Indigo
    color_end = (15, 20, 45) # Dark Blue
    
    for y in range(height):
        r = int(color_start[0] + (color_end[0] - color_start[0]) * (y / height))
        g = int(color_start[1] + (color_end[1] - color_start[1]) * (y / height))
        b = int(color_start[2] + (color_end[2] - color_start[2]) * (y / height))
        draw.line([(0, y), (width, y)], fill=(r, g, b))
        
    # Frames
    draw.rectangle([15, 15, width - 16, height - 16], outline=(255, 255, 255, 40), width=1)
    draw.rectangle([20, 20, width - 21, height - 21], outline=(255, 255, 255, 120), width=2)
    
    # Fonts: Arialbd.ttf is guaranteed on Windows
    font_path = r"C:\Windows\Fonts\arialbd.ttf"
    try:
        font_title = ImageFont.truetype(font_path, 20)
        font_author = ImageFont.truetype(font_path, 14)
        font_logo = ImageFont.truetype(font_path, 11)
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
        # check width
        w = draw.textlength(test_line, font=font_title) if hasattr(draw, "textlength") else len(test_line) * 10
        if w > 240:
            current_line.pop()
            lines.append(" ".join(current_line))
            current_line = [word]
    if current_line:
        lines.append(" ".join(current_line))
        
    y_text = height // 2 - (len(lines) * 15)
    for line in lines:
        draw.text((width // 2, y_text), line, fill=(255, 255, 255), font=font_title, anchor="mm")
        y_text += 30
        
    # Author
    draw.text((width // 2, height - 60), author, fill=(230, 230, 230), font=font_author, anchor="mm")
    
    # Emblem
    draw.ellipse([width // 2 - 12, height - 110, width // 2 + 12, height - 86], outline=(255, 255, 255, 100), width=1)
    draw.text((width // 2, height - 98), "B", fill=(255, 255, 255, 150), font=font_logo, anchor="mm")
    
    image.save(filepath, "JPEG", quality=95)

headers = {
    'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36',
    'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8',
    'Accept-Language': 'en-US,en;q=0.5'
}

session = requests.Session()
session.headers.update(headers)

for book in books_search:
    filename = book["filename"]
    query = book["query"]
    title = book["title"]
    author = book["author"]
    filepath = os.path.join(target_dir, filename)
    
    print(f"Searching cover for: {query}...")
    search_url = f"https://www.bing.com/images/search?q={urllib.parse.quote(query)}"
    
    success = False
    try:
        r = session.get(search_url, timeout=10)
        r.raise_for_status()
        
        # Parse image URLs from Bing markup
        murls = re.findall(r'murl&quot;:&quot;(http[^&"]+)&quot;', r.text)
        if not murls:
            murls = re.findall(r'murl[^\w]+(http[^\&"\s]+)', r.text)
            
        if murls:
            # Filter for safe and trusted URLs first
            trusted_urls = [u for u in murls if is_safe_and_trusted(u)]
            
            # Try to download from trusted URLs
            for img_url in trusted_urls[:5]:
                print(f"  Trying trusted URL: {img_url}")
                try:
                    img_res = session.get(img_url, timeout=10)
                    img_res.raise_for_status()
                    
                    with open(filepath, 'wb') as f:
                        f.write(img_res.content)
                        
                    with Image.open(filepath) as img:
                        img.verify()
                        
                    print(f"  Successfully downloaded real cover: {filename}")
                    success = True
                    break
                except Exception as e:
                    print(f"  Failed download: {e}")
                    if os.path.exists(filepath):
                        os.remove(filepath)
                        
    except Exception as e:
        print(f"Search failed for {query}: {e}")
        
    if not success:
        # Fallback: Generate a high-quality clean cover using Arial (supports Vietnamese diacritics)
        print(f"  Falling back to generated cover with Arial font for: {title}")
        generate_beautiful_cover(filepath, title, author)

print("Safe covers preparation finished!")
