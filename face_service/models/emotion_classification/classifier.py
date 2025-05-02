from PIL import Image
from transformers import ViTForImageClassification
from transformers import pipeline

model_path = './model/'

pipe = pipeline('image-classification', model=model_path, device=0)

image_path = "./test/in.png"
image = Image.open(image_path)

print(pipe(image))