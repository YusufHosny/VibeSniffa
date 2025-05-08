from PIL import Image, ImageDraw
import requests
import io
import json

base_url = "http://localhost:8000"

# test 1
img_path = "in_1.jpg"
image = Image.open(img_path).convert("RGB")
buf = io.BytesIO()
image.save(buf, format='JPEG')
buf.seek(0)
files = {'file': ('in.jpg', buf, 'image/jpeg')}

print("Sending to /update_bounds...")
resp1 = requests.post(f"{base_url}/update_bounds", files=files)
print("Response from /update_bounds:")
print(resp1.json())

draw = ImageDraw.Draw(image)
for rect in resp1.json()['bounds']:
    draw.rectangle(rect, outline="red", width=3)
image.show()

# test 2
img_path = "in_2.png"
image = Image.open(img_path).convert("RGB")
buf = io.BytesIO()
image.save(buf, format='JPEG')
buf.seek(0)
files = {'file': ('in.png', buf, 'image/png')}

print("\nSending to /get_emotion...")
resp2 = requests.post(f"{base_url}/get_emotion", files=files)
print("Response from /get_emotion:")
print(resp2.json())
