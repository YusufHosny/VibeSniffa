using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ImageUploader : MonoBehaviour
{
    public Texture2D imageTexture; 
    public string apiUrl = "url/get_emotion"; // FastAPI /get_emotion endpoint
    
    public void UploadImage()
    {
        StartCoroutine(SendImageToAPI());
    }

    private IEnumerator SendImageToAPI()
    {
        // Validate image
        if (imageTexture == null)
        {
            Debug.LogError("Image texture is null. Please assign a valid image.");
            yield break;
        }

        // Encode the image to PNG
        byte[] imageBytes = imageTexture.EncodeToPNG();
        Debug.Log($"Image bytes length: {imageBytes.Length}");

        // Save image for debugging
        System.IO.File.WriteAllBytes("test.png", imageBytes);
        Debug.Log("Image saved to test.png for debugging");

        // Create a form and add the image
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", imageBytes, "image.png", "image/png");

        // Send the POST request
        using (UnityWebRequest www = UnityWebRequest.Post(apiUrl, form))
        {
            yield return www.SendWebRequest();

            // Check the result
            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = www.downloadHandler.text;
                Debug.Log($"Raw response: {json}");
                try
                {
                    EmotionResult result = JsonUtility.FromJson<EmotionResult>(json);
                    Debug.Log($"Main Emotion: {result.main_emotion}");
                    Debug.Log($"Description: {result.description}");
                    // Example: Display in UI
                    // uiText.text = $"Emotion: {result.main_emotion}\n{result.description}";
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to parse JSON response: {e.Message}");
                    Debug.LogError($"Raw response: {json}");
                    // Fallback message
                    // uiText.text = "Error: Could not process response.";
                }
            }
            else
            {
                Debug.LogError($"Error uploading image: {www.error}");
                Debug.LogError($"Response code: {www.responseCode}");
                Debug.LogError($"Raw response: {www.downloadHandler.text}");
                // Fallback message
                // uiText.text = "Error: Server failed to process the image.";
            }
        }
    }
}

[System.Serializable]
public class EmotionResult
{
    public string main_emotion;
    public string description;
}

// Optional: For parsing error responses from FastAPI
[System.Serializable]
public class ErrorResponse
{
    public string detail;
}