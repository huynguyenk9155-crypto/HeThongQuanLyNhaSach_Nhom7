import os
import urllib.request
import ssl

ssl._create_default_https_context = ssl._create_unverified_context

covers = {
    "DacNhanTam.jpg": "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1360155255i/17325515.jpg",
    "NhaGiaKim.jpg": "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1483412268i/8633979.jpg",
    "TuDuyNhanhCham.jpg": "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1327954152i/11468377.jpg",
    "Sapiens.jpg": "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1590278963i/53543794.jpg",
    "atomic.jpg": "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1655988006i/40121378.jpg",
    "DamBiGhet.jpg": "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1463131713i/30186981.jpg",
    "SoDo.jpg": "https://upload.wikimedia.org/wikipedia/vi/8/8e/Sodobanindau.jpg",
    "RungNaUy.jpg": "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1344498305i/15809706.jpg",
    "ChaGiauChaNgheo.jpg": "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1583215689i/50196396.jpg",
    "QuocGiaKhoiNghiep.jpg": "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1367375266i/17877209.jpg",
    "VuTru.jpg": "https://upload.wikimedia.org/wikipedia/en/c/c3/Cosmos_Carl_Sagan.jpg",
    "LuocSuThoiGian.jpg": "https://upload.wikimedia.org/wikipedia/en/a/a3/BriefHistoryTime.jpg",
    "DoiThayDoiKhiChungTaThayDoi.jpg": "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1523425575i/39810356.jpg",
    "KheoAnNoiSeCoDuocThienHa.jpg": "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1460301138i/29889410.jpg",
    "SuViet12KhucTrangCa.jpg": "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1506509935i/36318991.jpg",
    "BaoTapTrieuTran.jpg": "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1414777598i/23491321.jpg",
    "YTuongNayLaCuaChungMinh.jpg": "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1408892631i/23013898.jpg",
    "HieuVeNgheThuat.jpg": "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1603598711i/55767554.jpg",
    "PragmaticProgrammer.jpg": "https://upload.wikimedia.org/wikipedia/en/8/8f/The_pragmatic_programmer.jpg",
    "DesigningDataIntensive.jpg": "https://learning.oreilly.com/library/cover/9781491903063/250w/",
    "HieuVeTraiTim.jpg": "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1360677591i/17349175.jpg",
    "NgheThuatTuDuyRanhMach.jpg": "https://images-na.ssl-images-amazon.com/images/S/compressed.photo.goodreads.com/books/1429532585i/25394200.jpg"
}

headers = {'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64)'}

for filename, url in covers.items():
    print(f"Downloading cover for {filename}...")
    try:
        req = urllib.request.Request(url, headers=headers)
        with urllib.request.urlopen(req) as response:
            data = response.read()
            # Save to Images/
            dest1 = os.path.join("c:\\Tuan6\\Images", filename)
            with open(dest1, "wb") as f:
                f.write(data)
            # Save to wwwroot/images/
            dest2 = os.path.join("c:\\Tuan6\\wwwroot\\images", filename)
            with open(dest2, "wb") as f:
                f.write(data)
            print(f"Successfully downloaded {filename}")
    except Exception as e:
        print(f"Error downloading {filename}: {e}")

print("Download complete.")
