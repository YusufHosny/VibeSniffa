import openai

import os
import time
import random

from typing import Self, Dict
from dotenv import load_dotenv


class LLM_Generator:

    def __init__(self: Self, apikey: str|None = None):
        self._description = None
        self._activations = None

        env = load_dotenv('.env')

        if apikey is None:
            self._api_key = env["OPENAI_API_KEY"]
        else:
            self._api_key = apikey

        from textwrap import dedent
        self._prompt = dedent(
            """
            Given emotion activations from an emotion classification model (scores between 0 and 1), describe the person's mood in a short message (10 words or less).
            The message should be humorous and snappy, like a sentence a friend or an internet meme would use.

            Example 1:
            Emotions: happy: 0.6, sad: 0.0, angry: 0.0, fear: 0.0, surprise: 0.2, neutral: 0.2, disgust: 0.0
            Mood: Someone had a good shit this morning

            Example 2:
            Emotions: happy: 0.2, sad: 0.1, angry: 0.0, fear: 0.2, surprise: 0.0, neutral: 0.5, disgust: 0.3
            Mood: Bro did not get a good night's sleep

            Example 3:
            Emotions: happy: 0.9, sad: 0.1, angry: 0.0, fear: 0.0, surprise: 0.2, neutral: 0.0, disgust: 0.0
            Mood: They might be in love with you

            Example 4:
            Emotions: happy: 0.1, sad: 0.7, angry: 0.4, fear: 0.0, surprise: 0.2, neutral: 0.0, disgust: 0.4
            Mood: Don't come to school tomorrow

            Now, for the following emotions:
            """
        )

        


    def __call__(self: Self, emotion_activations: Dict):
        for emotion, score in emotion_activations.items():
            prompt += f"{emotion}: {score}, "
        prompt = prompt[:-2]
        prompt += "\nMood: "

        client = openai.OpenAI(api_key=self._api_key)
        max_retries = 3
        for attempt in range(max_retries):
            try:
                completion = client.completions.create(
                    model="gpt-3.5-turbo-instruct",
                    prompt=prompt,
                    max_tokens=50,
                    temperature=0.7,
                    stop=["\n"]
                )

                return completion.choices[0].text.strip()
            
            except openai.RateLimitError as e:
                if attempt < max_retries - 1:
                    wait_time = 2 ** attempt + random.uniform(0, 1)  # Exponential backoff
                    time.sleep(wait_time)
                    continue
                else:
                    return f"Rate limit exceeded: {e}"
            except openai.OpenAIError as e:
                return f"API error: {e}"

if __name__ == "__main__":
    emotions = {
        "happy": 0.9,
        "sad": 0.9,
        "angry": 0.9,
        "fear": 0.9,
        "surprise": 0.6,
        "neutral": 0.3,
        "disgust": 0.0 
    }

    model = LLM_Generator()

    mood_description = model(emotions)
    print(f"Mood: {mood_description}")