// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.ARSubsystems;
using static VibeSniffa.InferenceUiManager;

namespace VibeSniffa
{
    public class InferenceRunManager : MonoBehaviour
    {
        [Header("UI display references")]
        [SerializeField] private InferenceUiManager m_uiInference;
        [Header("Ui references")]
        [SerializeField] private DetectionUiMenuManager m_uiMenuManager;

        #region Inference Functions
        public string BaseApiUrl = "http://192.168.137.1/"; // url and port of service

        public void InferBounds(WebCamTexture texture)
        {
            if (!texture)
            {
                return;
            }

            var tex2d = new Texture2D(texture.width, texture.height);
            tex2d.SetPixels32(texture.GetPixels32());
            StartCoroutine(BoundsRoutine(tex2d));
        }

        private IEnumerator BoundsRoutine(Texture2D sourcetex)
        {
            var imageBytes = sourcetex.EncodeToPNG();

            // Create a form and add the image
            var form = new WWWForm();
            form.AddBinaryData("file", imageBytes, "image.png", "image/png");

            // Send the POST request
            var apiUrl = BaseApiUrl + "get_bounds/";
            using var www = UnityWebRequest.Post(apiUrl, form);
            yield return www.SendWebRequest();

            // Check the result
            if (www.result == UnityWebRequest.Result.Success)
            {
                var json = www.downloadHandler.text;
                Debug.Log($"Raw response: {json}");
                try
                {
                    var result = BoundsInference.FromJsonString(json);
                    m_uiInference.DrawUIBoxes(result, sourcetex);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse JSON response: {e.Message}");
                    Debug.LogError($"Raw response: {json}");
                    m_uiInference.OnObjectDetectionError();
                }
            }
            else
            {
                Debug.LogError($"Error uploading image: {www.error}");
                Debug.LogError($"Response code: {www.responseCode}");
                Debug.LogError($"Raw response: {www.downloadHandler.text}");
                m_uiInference.OnObjectDetectionError();
            }
        }

        public void InferEmotion(BoundingBox box, bool force)
        {
            StartCoroutine(EmotionRoutine(box, force));
        }

        private IEnumerator EmotionRoutine(BoundingBox box, bool force)
        {
            var imageBytes = box.FaceScreenshot.EncodeToPNG();

            // Create a form and add the image
            var form = new WWWForm();
            form.AddBinaryData("file", imageBytes, "image.png", "image/png");

            // Send the POST request
            var apiUrl = BaseApiUrl + $"get_emotion/{force}";
            using var www = UnityWebRequest.Post(apiUrl, form);
            yield return www.SendWebRequest();

            // Check the result
            if (www.result == UnityWebRequest.Result.Success)
            {
                var json = www.downloadHandler.text;
                Debug.Log($"Raw response: {json}");
                try
                {
                    var result = EmotionInference.FromJsonString(json);
                    m_uiInference.UpdateBoxWithEmotions(result, box);
                    Debug.Log(json);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse JSON response: {e.Message}");
                    Debug.LogError($"Raw response: {json}");
                    m_uiInference.OnObjectDetectionError();
                }
            }
            else
            {
                Debug.LogError($"Error uploading image: {www.error}");
                Debug.LogError($"Response code: {www.responseCode}");
                Debug.LogError($"Raw response: {www.downloadHandler.text}");
                m_uiInference.OnObjectDetectionError();
            }
        }

        public void InferTest()
        {
            StartCoroutine(TestRoutine());
        }

        private IEnumerator TestRoutine()
        {
            // Send the get request
            var apiUrl = BaseApiUrl;
            Debug.Log($"Attempting Test Request to: {apiUrl}");
            using var www = UnityWebRequest.Get(apiUrl);
            yield return www.SendWebRequest();

            // Check the result
            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = www.downloadHandler.text;
                Debug.Log($"Raw response: {response}");
            }
            else
            {
                Debug.Log($"Error testing: {www.error}");
                Debug.Log($"Response code: {www.responseCode}");
                Debug.Log($"Raw response: {www.downloadHandler.text}");
                m_uiInference.OnObjectDetectionError();

            }
        }
        #endregion
    }
}
