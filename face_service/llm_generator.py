import openai

import time
import random

from typing import Self, Dict
from dotenv import dotenv_values


class LLM_Generator:

    def __init__(self: Self, apikey: str|None = None):
        self._description = None
        self._activations = None
        self._last_updated = 0

        env = dotenv_values('.env')

        if apikey is None:
            self._api_key = env["OPENAI_API_KEY"]
        else:
            self._api_key = apikey

        from textwrap import dedent
        self._prompt = dedent(
            """
            Given emotion activations from an emotion classification model (scores between 0 and 1), describe the person's mood in a short message (10 words or less).
            The message should be humorous, edgy, and snappy, with strong jokes and internet culture, like a sentence a friend or an internet meme would use.

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


    def _should_update(self: Self, activations: Dict):
        rate_limit_s = 5
        now = time.time()
        if now - self._last_updated < rate_limit_s:
            return False

        if None in (self._activations, self._description):
            return True
        
        if any([emotion not in self._activations for emotion in activations]):
            return True
        
        score_pairs = [(activations[emotion], self._activations[emotion]) for emotion in activations]
        distance = sum([ (score - oldscore)**2 for score, oldscore in score_pairs ])

        threshold = 1
        if distance > threshold:
            return True
        
        return False

    def __call__(self: Self, activations: Dict):

        if not self._should_update(activations): return
        self._last_updated = time.time()

        for emotion, score in activations.items():
            self._prompt += f"{emotion}: {"{:.2f}".format(score)}, "
        self._prompt = self._prompt[:-2]
        self._prompt += "\nMood: "

        client = openai.OpenAI(api_key=self._api_key)
        max_retries = 3
        for attempt in range(max_retries):
            try:
                completion = client.completions.create(
                    model="gpt-3.5-turbo-instruct",
                    prompt=self._prompt,
                    max_tokens=50,
                    temperature=0.7,
                    stop=["\n"]
                )

                self._activations = activations
                self._description = completion.choices[0].text.strip()
                self._last_updated = time.time()
                return self._description
            
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
    print(f"{mood_description}")