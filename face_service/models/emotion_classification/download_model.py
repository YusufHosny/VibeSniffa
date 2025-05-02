from transformers import ViTForImageClassification

model_str = "dima806/facial_emotions_image_detection"

labels_list = ['sad', 'disgust', 'angry', 'neutral', 'fear', 'surprise', 'happy']

model = ViTForImageClassification.from_pretrained(model_str, num_labels=len(labels_list))

model.save_pretrained('./model')