# Detector imports
import ultralytics

# Classifier imports
from transformers import pipeline

# Generator imports
from llm_generator import LLM_Generator

# Service imports
from PIL import Image
import supervision as sv
from typing import Self, Dict

class VibeSniffaModel:

    def __init__(self: Self):
        self._load_detector()
        self._load_classifier()
        self._load_llm_module()

    def _load_detector(self: Self, detector_path: str = './models/face_detection/model/model.pt'):
        self.detector = ultralytics.YOLO(detector_path)

    def _load_classifier(self: Self, classifier_path: str ='./models/emotion_classification/model/'):
        self.classifier = pipeline('image-classification', model=classifier_path, device=0)

    def _load_llm_module(self: Self):
        self.llm = LLM_Generator()

    def detect(self: Self, image: Image):
        return sv.Detections.from_ultralytics(self.detector(image)[0]).xyxy.tolist()

    def classify(self: Self, image: Image):
        return {activation['label']: activation['score'] for activation in self.classifier(image)}
    
    def generate_string(self: Self, activations: Dict):
        return self.llm(activations)

    def __call__(self: Self, *args, **kwds):
        image = args[0] # need to define type

        xyxys = self.detect(image)
        cropped_images = [sv.crop_image(image=image, xyxy=xyxy) for xyxy in xyxys]
        emotions = [self.classify(cropped) for cropped in cropped_images]

        output = [
            {
                'bounds': xyxy,
                'emotions': emotion
            }
            for xyxy, emotion in zip(xyxys, emotions)
        ]

        return output
    