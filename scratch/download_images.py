import os
import sys
import urllib.request

# Reconfigure stdout for UTF-8 to prevent encoding errors on Windows
if sys.stdout.encoding != 'utf-8':
    sys.stdout.reconfigure(encoding='utf-8')

images = {
    "ChoToiXinMotVeDiTuoiTho.jpg": "https://upload.wikimedia.org/wikipedia/vi/b/ba/Cho_t%C3%B4i_xin_m%E1%BB%99t_v%C3%A9_%C4%91i_tu%E1%BB%95i_th%C6%A1.jpg",
    "TatDen.jpg": "https://upload.wikimedia.org/wikipedia/vi/8/87/T%E1%BA%AFt_%C4%91%C3%A8n-Nh%C3%A3_Nam.jpeg",
    "MatBiec.jpg": "https://upload.wikimedia.org/wikipedia/vi/e/ee/M%E1%B3%AFt_bi%E1%BA%BFc.jpg",
    "ChiPheo.jpg": "https://upload.wikimedia.org/wikipedia/vi/a/a2/Chi_Pheo_cover_1957.jpg",
    "CasioCalculator.jpg": "https://upload.wikimedia.org/wikipedia/commons/3/3d/Casio_fx-991ES_Calculator_New.jpg",
    "ColoredPencils.jpg": "https://upload.wikimedia.org/wikipedia/commons/e/e0/Colored-Pencils.jpg",
    "BallpointPen.jpg": "https://upload.wikimedia.org/wikipedia/commons/f/fc/Blue_ballpoint_pen.jpg",
    "WoodenRuler.jpg": "https://upload.wikimedia.org/wikipedia/commons/d/de/Ruler.jpg",
    "NotebookSpiral.jpg": "https://upload.wikimedia.org/wikipedia/commons/b/b5/Notebook_spiral.jpg"
}

# Directories
src_dir = r"c:\Tuan6\Images"
www_dir = r"c:\Tuan6\wwwroot\images"

os.makedirs(src_dir, exist_ok=True)
os.makedirs(www_dir, exist_ok=True)

# Add custom User-Agent to bypass potential bot filters
opener = urllib.request.build_opener()
opener.addheaders = [('User-Agent', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36')]
urllib.request.install_opener(opener)

for name, url in images.items():
    try:
        print(f"Downloading {name} from {url}...")
        # Download to src_dir
        dest_src = os.path.join(src_dir, name)
        urllib.request.urlretrieve(url, dest_src)
        
        # Download to www_dir
        dest_www = os.path.join(www_dir, name)
        urllib.request.urlretrieve(url, dest_www)
        print(f"Downloaded {name} successfully.")
    except Exception as e:
        print(f"Failed to download {name}: {e}")

print("Done downloading all images.")
