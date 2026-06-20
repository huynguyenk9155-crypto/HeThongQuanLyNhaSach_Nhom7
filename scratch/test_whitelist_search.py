import requests
import re
import urllib.parse

WHITELIST_DOMAINS = [
    'tikicdn.com', 'shopee.vn', 'lazcdn.com', 'fahasa.com', 'wikimedia.org',
    'goodreads.com', 'nhanam.com.vn', 'nxbkimdong.com.vn', 'nxbtre.com.vn',
    'ybook.vn', 'vinabook.com', 'alphabooks.vn'
]

def is_whitelisted(url):
    return any(domain in url.lower() for domain in WHITELIST_DOMAINS)

query = "Kheo An Noi Se Co Duoc Thien Ha Trac Nha bia sach"
search_url = f"https://www.bing.com/images/search?q={urllib.parse.quote(query)}"
headers = {'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'}

r = requests.get(search_url, headers=headers)
murls = re.findall(r'murl&quot;:&quot;(http[^&"]+)&quot;', r.text)
if not murls:
    murls = re.findall(r'murl[^\w]+(http[^\&"\s]+)', r.text)

print("All found URLs:")
for u in murls[:10]:
    print(f"  {u} (Whitelisted: {is_whitelisted(u)})")
