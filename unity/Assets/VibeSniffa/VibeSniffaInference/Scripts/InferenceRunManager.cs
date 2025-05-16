// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using UnityEngine.Networking;

namespace VibeSniffa
{
    public class InferenceRunManager : MonoBehaviour
    {
        [Header("UI display references")]
        [SerializeField] private InferenceUiManager m_uiInference;
        [Header("Ui references")]
        [SerializeField] private DetectionUiMenuManager m_uiMenuManager;
        [SerializeField] private Vector2Int m_inputSize = new(640, 640);

        #region Inference Functions
        public string BaseApiUrl = "http://192.168.137.1/"; // url and port of service

        public void InferBounds(WebCamTexture texture)
        {
            if (!texture)
            {
                return;
            }
            m_uiInference.SetDetectionCapture(texture);

            var tex2d = new Texture2D(texture.width, texture.height);
            tex2d.SetPixels32(texture.GetPixels32());
            m_inputSize.x = texture.width; m_inputSize.y = texture.height;
            var imageBytes = tex2d.EncodeToPNG();
            StartCoroutine(BoundsRoutine(imageBytes));
        }

        private IEnumerator BoundsRoutine(byte[] imageBytes)
        {
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
                    m_uiInference.DrawUIBoxes(result, m_inputSize.x, m_inputSize.y);
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

        public void InferEmotion(WebCamTexture texture)
        {
            if (!texture)
            {
                return;
            }
            m_uiInference.SetDetectionCapture(texture);

            var tex2d = new Texture2D(texture.width, texture.height);
            tex2d.SetPixels32(texture.GetPixels32());
            var imageBytes = tex2d.EncodeToPNG();
            StartCoroutine(BoundsRoutine(imageBytes));
        }

        private IEnumerator EmotionRoutine(byte[] imageBytes)
        {
            // Create a form and add the image
            var form = new WWWForm();
            form.AddBinaryData("file", imageBytes, "image.png", "image/png");

            // Send the POST request
            var apiUrl = BaseApiUrl + "get_emotion/";
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
            m_uiMenuManager.AddDebugMsg($"Attempting Test Request to: {apiUrl}");
            using var www = UnityWebRequest.Get(apiUrl);
            yield return www.SendWebRequest();

            // Check the result
            if (www.result == UnityWebRequest.Result.Success)
            {
                var response = www.downloadHandler.text;
                m_uiMenuManager.AddDebugMsg($"Raw response: {response}");
            }
            else
            {
                m_uiMenuManager.AddDebugMsg($"Error testing: {www.error}");
                m_uiMenuManager.AddDebugMsg($"Response code: {www.responseCode}");
                m_uiMenuManager.AddDebugMsg($"Raw response: {www.downloadHandler.text}");
                m_uiInference.OnObjectDetectionError();

            }
        }
        #endregion
    }
}
