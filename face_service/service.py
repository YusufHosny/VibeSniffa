from fastapi import FastAPI, UploadFile, File
from pydantic import BaseModel
from typing import List
from PIL import Image
import io

from model import VibeSniffaModel

app = FastAPI()
model = VibeSniffaModel()

class BoundsResult(BaseModel):
    bounds: List[List[float]]
    
class EmotionResult(BaseModel):
    emotion: str
    description: str

@app.post("/get_bounds", response_model=BoundsResult)
async def get_bounds(file: UploadFile = File(...)):
    image_data = await file.read()
    image = Image.open(io.BytesIO(image_data)).convert("RGB")

    xyxys = model.detect(image)

    result = BoundsResult(
        bounds=xyxys
    )

    return result

@app.post("/get_emotion", response_model=EmotionResult)
async def get_emotion(file: UploadFile = File(...)):
    image_data = await file.read()
    image = Image.open(io.BytesIO(image_data)).convert("RGB")

    emotions = model.classify(image)
    main_emotion = max(emotions, key=emotions.get)
    description = model.generate_string(emotions)

    result = EmotionResult(
        emotion=main_emotion,
        description=description
    )

    return result
