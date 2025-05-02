import ultralytics
import supervision as sv
from PIL import Image
from matplotlib import pyplot as plt

model_path = './model/model.pt'

model = ultralytics.YOLO(model_path)

image_path = "./test/in.jpg"
image = Image.open(image_path)
image = image.resize((600, int(image.height/image.width * 600)))
output = model(image)
detections = sv.Detections.from_ultralytics(output[0])

box_annotator = sv.BoxAnnotator()
label_annotator = sv.LabelAnnotator()

annotated_image = box_annotator.annotate(
    scene=image, detections=detections)
annotated_image = label_annotator.annotate(
    scene=annotated_image, detections=detections)
with sv.ImageSink(target_dir_path='./test/', image_name_pattern="out{:01d}.png") as sink:
        for xyxy in detections.xyxy:
            cropped_image = sv.crop_image(image=image, xyxy=xyxy)
            sink.save_image(image=cropped_image)

plt.imshow(annotated_image)
plt.show()