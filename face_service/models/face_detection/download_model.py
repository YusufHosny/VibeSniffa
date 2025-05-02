from huggingface_hub import hf_hub_download

model_path = hf_hub_download(repo_id="arnabdhar/YOLOv8-Face-Detection", filename="model.pt", local_dir='./model/')

print(f'model downloaded at {model_path}')