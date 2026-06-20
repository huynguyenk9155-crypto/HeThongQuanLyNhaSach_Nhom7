import requests
import re

query = "So Do Vu Trong Phung bia sach"
url = f"https://www.google.com/search?q={query}&tbm=isch"
headers = {'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36'}

r = requests.get(url, headers=headers)
with open(r"c:\Tuan6\scratch\google.html", "w", encoding="utf-8") as f:
    f.write(r.text)
print("Wrote html file.")



