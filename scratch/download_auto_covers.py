import os
import sys
import re
import urllib.parse
import requests
from PIL import Image

# Reconfigure stdout for UTF-8 to prevent encoding errors on Windows
if sys.stdout.encoding != 'utf-8':
    sys.stdout.reconfigure(encoding='utf-8')

target_dir = r"c:\Tuan6\wwwroot\images"
os.makedirs(target_dir, exist_ok=True)

books_search = [
    {"filename": "SoDo.jpg", "query": "So Do Vu Trong Phung bia sach"},
    {"filename": "RungNaUy.jpg", "query": "Rung Na Uy Haruki Murakami bia sach"},
    {"filename": "ChaGiauChaNgheo.jpg", "query": "Cha Giau Cha Ngheo Robert Kiyosaki bia sach"},
    {"filename": "QuocGiaKhoiNghiep.jpg", "query": "Quoc Gia Khoi Nghiep Dan Senor bia sach"},
    {"filename": "VuTru.jpg", "query": "Vu Tru Carl Sagan bia sach"},
    {"filename": "LuocSuThoiGian.jpg", "query": "Luoc Su Thoi Gian Stephen Hawking bia sach"},
    {"filename": "DoiThayDoiKhiChungTaThayDoi.jpg", "query": "Doi Thay Doi Khi Chung Ta Thay Doi Andrew Matthews bia sach"},
    {"filename": "KheoAnNoiSeCoDuocThienHa.jpg", "query": "Kheo An Noi Se Co Duoc Thien Ha Trac Nha bia sach"},
    {"filename": "SuViet12KhucTrangCa.jpg", "query": "Su Viet 12 Khuc Trang Ca Dung Phan bia sach"},
    {"filename": "BaoTapTrieuTran.jpg", "query": "Bao Tap Trieu Tran Hoang Quoc Hai bia sach"},
    {"filename": "KinhVanHoa.jpg", "query": "Kinh Van Hoa Nguyen Nhat Anh bia sach"},
    {"filename": "KhongGiaDinh.jpg", "query": "Khong Gia Dinh Hector Malot bia sach"},
    {"filename": "HackNao1500Tu.jpg", "query": "Hack Nao 1500 Tu Tieng Anh bia sach"},
    {"filename": "LuyenSieuTriNhoTuVung.jpg", "query": "Luyen Sieu Tri Nho Tu Vung Tieng Anh bia sach"},
    {"filename": "HieuVeTraiTim.jpg", "query": "Hieu Ve Trai Tim Minh Niem bia sach"},
    {"filename": "NgheThuatTuDuyRanhMach.jpg", "query": "Nghe Thuat Tu Duy Ranh Mach Rolf Dobelli bia sach"},
    {"filename": "PragmaticProgrammer.jpg", "query": "The Pragmatic Programmer book cover"},
    {"filename": "DesigningDataIntensive.jpg", "query": "Designing Data Intensive Applications book cover"},
    {"filename": "YTuongNayLaCuaChungMinh.jpg", "query": "Y Tuong Nay Le Cua Chung Minh Huynh Vinh Son bia sach"},
    {"filename": "HieuVeNgheThuat.jpg", "query": "Hieu Ve Nghe Thuat bia sach"}
]

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
    filepath = os.path.join(target_dir, filename)
    
    print(f"Searching cover for: {query}...")
    search_url = f"https://www.bing.com/images/search?q={urllib.parse.quote(query)}"
    
    try:
        r = session.get(search_url, timeout=10)
        r.raise_for_status()
        
        # Parse image URLs from Bing markup
        murls = re.findall(r'murl&quot;:&quot;(http[^&"]+)&quot;', r.text)
        if not murls:
            murls = re.findall(r'murl[^\w]+(http[^\&"\s]+)', r.text)
            
        if not murls:
            print(f"No image results found for query: {query}")
            continue
            
        success = False
        # Try top 5 URLs to find a working one
        for img_url in murls[:5]:
            print(f"  Trying download from: {img_url}")
            try:
                img_res = session.get(img_url, timeout=10)
                img_res.raise_for_status()
                
                # Temporary save and validation
                with open(filepath, 'wb') as f:
                    f.write(img_res.content)
                    
                # Validate image is not corrupted
                with Image.open(filepath) as img:
                    img.verify()
                    
                print(f"  Successfully downloaded and verified: {filename}")
                success = True
                break
            except Exception as e:
                print(f"  Failed download from {img_url}: {e}")
                if os.path.exists(filepath):
                    os.remove(filepath)
                    
        if not success:
            print(f"Could not download any valid image for: {query}")
            
    except Exception as e:
        print(f"Search failed for {query}: {e}")

print("Auto download finished!")
