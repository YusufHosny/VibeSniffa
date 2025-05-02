import ultralytics
import supervision as sv
import cv2 as cv

model_path = './model.pt'

model = ultralytics.YOLO(model_path)

image_path = "./test/in.jpg"
image = cv.imread(image_path)
image = cv.resize(image, (0, 0), fx=0.2, fy=0.2)
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

cv.imshow("img", annotated_image)
cv.waitKey(10000)