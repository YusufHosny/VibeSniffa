import openai
import os
import time
import random

api_key = os.getenv("OPENAI_API_KEY", "")

def get_mood_description(emotion_activations):
    """
    Query OpenAI API to generate a mood description based on emotion activations.
    
    Args:
        emotion_activations (dict): Dictionary with emotion names and scores (0 to 1).
                                   Example: {"Happiness": 0.7, "Sadness": 0.2, ...}
    
    Returns:
        str: A short mood description (e.g., "Very happy").
    """
    # Define the prompt
    prompt = """
Given emotion activations (scores between 0 and 1), describe the person's mood in a short message (10 words or less).

Example 1:
Emotions: Happiness: 0.9, Sadness: 0.1, Anger: 0.0, Fear: 0.0, Surprise: 0.2, Disgust: 0.0
Mood: Very happy

Example 2:
Emotions: Happiness: 0.2, Sadness: 0.8, Anger: 0.3, Fear: 0.1, Surprise: 0.4, Disgust: 0.5
Mood: Sad and disgusted with a hint of anger

Example 3:
Emotions: Happiness: 0.5, Sadness: 0.5, Anger: 0.0, Fear: 0.0, Surprise: 0.0, Disgust: 0.0
Mood: Mixed feelings, both happy and sad

Now, for the following emotions:
"""

    # Add the current emotion activations to the prompt
    for emotion, score in emotion_activations.items():
        prompt += f"{emotion}: {score}, "
    prompt = prompt[:-2]  # Remove trailing comma and space
    prompt += "\nMood: "

    # Query the OpenAI API with retry logic for rate limits
    client = openai.OpenAI(api_key=api_key)
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
            # Extract and return the mood description
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
        "Happiness": 0.9,
        "Sadness": 0.9,
        "Anger": 0.9,
        "Fear": 0.9,
        "Surprise": 0.6,
        "Disgust": 0.0 
    }
    mood_description = get_mood_description(emotions)
    print(f"Mood: {mood_description}")