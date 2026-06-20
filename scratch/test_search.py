import requests
import re

query = "So Do Vu Trong Phung bia sach"
url = f"https://images.search.yahoo.com/search/images?p={query}"
headers = {'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'}

r = requests.get(url, headers=headers)
urls = re.findall(r'"iurl":"(http[^"]+)"', r.text)
print("Found URLs:", urls[:3])
