import requests
import re
from urllib.parse import unquote

query = "So Do Vu Trong Phung bia sach"
url = f"https://www.bing.com/images/search?q={query}"
headers = {'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36'}

r = requests.get(url, headers=headers)
r = requests.get(url, headers=headers)
# Find murl matching HTML entities
murls = re.findall(r'murl&quot;:&quot;(http[^&]+)&quot;', r.text)
if not murls:
    # Try alternate regex for safety
    murls = re.findall(r'murl[^\w]+(http[^\&"\s]+)', r.text)
print("Found Bing murls:", murls[:5])


