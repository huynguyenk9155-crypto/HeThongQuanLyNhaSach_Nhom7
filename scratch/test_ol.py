import requests

books = [
    "So Do Vu Trong Phung",
    "Rung Na Uy Haruki Murakami",
    "Cha Giau Cha Ngheo",
    "Quoc Gia Khoi Nghiep",
    "Vu Tru Carl Sagan",
    "Luoc Su Thoi Gian Stephen Hawking",
    "Doi Thay Doi Khi Chung Ta Thay Doi",
    "Kheo An Noi Se Co Duoc ThienHa",
    "Su Viet 12 Khuc Trang Ca",
    "Bao Tap Trieu Tran Hoang Quoc Hai",
    "Kinh Van Hoa Nguyen Nhat Anh",
    "Khong Gia Dinh Hector Malot",
    "Hack Nao 1500 Tu Tieng Anh",
    "Luyen Sieu Tri Nho Tu Vung",
    "Hieu Ve TraiTim Minh Niem",
    "Nghe Thuat Tu Duy Ranh Mach",
    "The Pragmatic Programmer",
    "Designing Data-Intensive Applications",
    "Y Tuong Nay La Cua Chung Minh",
    "Hieu Ve Nghe Thuat"
]

for title in books:
    print(f"Searching OpenLibrary for: {title}...")
    try:
        r = requests.get(f"https://openlibrary.org/search.json?q={requests.utils.quote(title)}", timeout=10)
        data = r.json()
        docs = data.get("docs", [])
        found = False
        for doc in docs[:3]:
            cover_i = doc.get("cover_i")
            if cover_i:
                cover_url = f"https://covers.openlibrary.org/b/id/{cover_i}-L.jpg"
                print(f"  Found cover: {cover_url} for title: {doc.get('title')}")
                found = True
                break
        if not found:
            print("  No cover found.")
    except Exception as e:
        print(f"  Error: {e}")
