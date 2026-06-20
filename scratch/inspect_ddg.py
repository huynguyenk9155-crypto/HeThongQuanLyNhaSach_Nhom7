import requests

query = "So Do Vu Trong Phung bia sach"
url = f"https://html.duckduckgo.com/html/?q={query}"
headers = {'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'}

r = requests.get(url, headers=headers)
with open(r"c:\Tuan6\scratch\ddg.html", "w", encoding="utf-8") as f:
    f.write(r.text)
print("Wrote ddg html, status:", r.status_code, "size:", len(r.text))
