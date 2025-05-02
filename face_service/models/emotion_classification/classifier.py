from PIL import Image
from torchvision.transforms import Normalize
from transformers import ( 
    ViTImageProcessor,  
    ViTForImageClassification,
)
from transformers import pipeline

model_path = './model/'

processor = ViTImageProcessor.from_pretrained(model_path)
image_mean, image_std = processor.image_mean, processor.image_std
normalize = Normalize(mean=image_mean, std=image_std)

labels_list = ['sad', 'disgust', 'angry', 'neutral', 'fear', 'surprise', 'happy'] 

model = ViTForImageClassification.from_pretrained(model_path, num_labels=len(labels_list))
model.config.id2label = {ix: label for ix, label in enumerate(labels_list)}
model.config.label2id = {label: ix for ix, label in enumerate(labels_list)}

pipe = pipeline('image-classification', model=model_path, device=0)

image_path = "./test/in.png"
image = Image.open(image_path)

print(pipe(image))