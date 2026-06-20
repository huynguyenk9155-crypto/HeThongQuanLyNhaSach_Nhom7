import requests

url = "https://salt.tikicdn.com/ts/product/b2/24/09/b22409b30c793ff8bb2cb2a01d51c062.jpg"
headers = {
    "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
}
try:
    r = requests.get(url, headers=headers, timeout=10)
    print("Status code:", r.status_code)
    print("Content length:", len(r.content))
    print("Content text:", r.text)
except Exception as e:
    print("Error:", e)
