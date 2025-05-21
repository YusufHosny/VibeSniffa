// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Internal.Filters;
using PassthroughCamera;
using UnityEngine;
using UnityEngine.UI;

namespace VibeSniffa
{
    public class InferenceUiManager : MonoBehaviour
    {
        [Header("Placement configureation")]
        [SerializeField] private WebCamTextureManager m_webCamTextureManager;
        public PassthroughCameraEye CameraEye => m_webCamTextureManager.Eye;

        [Header("UI display references")]
        [SerializeField] private DetectionUiMenuManager m_uiMenuManager;
        [SerializeField] private Sprite m_boxTexture;
        [SerializeField] private Color m_boxColor;
        [SerializeField] private Font m_font;
        [SerializeField] private Color m_fontColor;
        [SerializeField] private int m_fontSize = 80;
        [SerializeField] private GameObject m_infoPanelPrefab;
        [Space(10)]
        public List<BoundingBox> BoxDrawn = new();
        private BoundingBox[] m_oldBoxDrawn = { };
        public List<GameObject> BoxPool = new();

        //bounding box data
        public class BoundingBox
        {
            public Vector3 WorldPos;
            public string ClassName;
            public string Description;
            public Texture2D FaceScreenshot;
            public bool DirtyBit;
        }

        #region Detection Functions
        public void OnObjectDetectionError()
        {
            ClearAnnotations();
        }
        #endregion

        #region BoundingBoxes functions

        public void DrawUIBoxes(List<BoundsInference> output, Texture2D source)
        {
            // Clear current boxes
            ClearAnnotations();

            var intrinsics = PassthroughCameraUtils.GetCameraIntrinsics(CameraEye);
            var displayWidth = intrinsics.Resolution.x;
            var displayHeight = intrinsics.Resolution.y;

            var scaleX = displayWidth / source.width;
            var scaleY = displayHeight / source.height;
            Debug.Log($"Scales: ({scaleX}, {scaleY})");

            var halfWidth = displayWidth / 2;
            var halfHeight = displayHeight / 2;

            var boxesFound = output.Count;
            var maxBoxes = Mathf.Min(boxesFound, 200);

            var camRes = intrinsics.Resolution;

            //Draw the bounding boxes
            for (var n = 0; n < maxBoxes; n++)
            {
                // Get bounding box center coordinates
                var centerX = output[n].GetCenterX() * scaleX - halfWidth;
                var centerY = output[n].GetCenterY() * scaleY - halfHeight;
                var perX = (centerX + halfWidth) / displayWidth;
                var perY = (centerY + halfHeight) / displayHeight;

                // Get the 3D marker world position using scaling method 
                var centerPixel = new Vector2Int(Mathf.RoundToInt(perX * camRes.x), Mathf.RoundToInt((1.0f - perY) * camRes.y));
                var ray = PassthroughCameraUtils.ScreenPointToRayInWorld(CameraEye, centerPixel);
                var worldPos = ProjectFaceToWorldSpace(ray, output[n].GetWidth());

                Debug.Log($"WorldPos: {worldPos}");

                // crop face from original texture
                var (originX, originY) = output[n].GetTopLeft();
                var (width, height) = output[n].GetDimensions();

                var c = source.GetPixels((int)originX, (int)originY, (int)width, (int)height);
                var cropped = new Texture2D((int)width, (int)height);
                cropped.SetPixels(c);
                cropped.Apply();

                var box = new BoundingBox
                {
                    WorldPos = worldPos,
                    FaceScreenshot = cropped,
                    ClassName = "",
                    Description = "",
                    DirtyBit = true
                };

                // check if box is close to an old box
                // if so, maintain old box
                var oldBoxSame =
                (m_oldBoxDrawn != null && m_oldBoxDrawn.Count() > 0)
                 ? m_oldBoxDrawn.ToList().Find(b => Vector3.Distance(worldPos, b.WorldPos) < .1)
                 : null;
                if (oldBoxSame != null)
                {
                    box.WorldPos = oldBoxSame.WorldPos;
                    box.ClassName = oldBoxSame.ClassName;
                    box.Description = oldBoxSame.Description;
                    box.DirtyBit = true;
                }

                // Add to the list of boxes
                BoxDrawn.Add(box);

                // Draw 2D box
                DrawBox(box, n);
            }
        }


        public void UpdateBoxWithEmotions(EmotionInference output, BoundingBox box)
        {
            var ix = BoxDrawn.FindIndex(b => Vector3.Distance(b.WorldPos, box.WorldPos) == BoxDrawn.Min(b => Vector3.Distance(b.WorldPos, box.WorldPos)));
            if (ix == -1)
            {
                Debug.Log("box not found");
                return;
            }
            box.ClassName = output.Emotion;
            box.Description = output.Description;

            DrawBox(box, ix);
        }

        private static Dictionary<string, Color> s_emotionColors =
        new()
        {
            { "sad", new Color(.1f, .45f, .95f, .8f) },
            { "disgust", new Color(.5f, .1f, .8f, .8f) },
            { "angry", new Color(.6f, .05f, .05f, .8f) },
            { "neutral", new Color(.6f, .6f, .6f, .8f) },
            { "fear", new Color(1f, .85f, .1f, .8f) },
            { "surprise", new Color(.95f, .5f, .1f, .8f) },
            { "happy", new Color(.1f, .95f, .1f, .8f) }
        };

        private void DrawBox(BoundingBox box, int id)
        {
            GameObject panel;
            if (id < BoxPool.Count)
            {
                panel = BoxPool[id];
                panel.SetActive(true);
            }
            else
            {
                panel = Instantiate(m_infoPanelPrefab);
                BoxPool.Add(panel);
            }

            var boxManager = panel.GetComponent<InferenceBoxManager>();
            if (boxManager != null)
            {
                var lookDir = (box.WorldPos - Camera.main.transform.position).normalized;
                var rot = Quaternion.LookRotation(lookDir, Vector3.up);
                boxManager.Initialize(box.WorldPos, rot);

                if (s_emotionColors.ContainsKey(box.ClassName))
                {
                    boxManager.ChangeColor(s_emotionColors[box.ClassName]);
                    boxManager.SetText($"{box.ClassName}: {box.Description}");
                }
            }
        }

        private void ClearAnnotations()
        {
            foreach (var box in BoxPool)
            {
                box?.SetActive(false);
            }
            m_oldBoxDrawn = BoxDrawn.ToArray();
            BoxDrawn.Clear();
        }

        private Vector3 ProjectFaceToWorldSpace(Ray ray, float targetWidthPixels)
        {
            var intrinsics = PassthroughCameraUtils.GetCameraIntrinsics(CameraEye);
            var screenWidth = intrinsics.Resolution.x;
            var fovRadiansX = 2f * Mathf.Atan(screenWidth / (2f * intrinsics.FocalLength.x));
            var focalLengthPixelsX = screenWidth / (2f * Mathf.Tan(fovRadiansX / 2f));
            var realWidthMeters = 0.18f;

            var distance = realWidthMeters * focalLengthPixelsX / targetWidthPixels;

            return ray.GetPoint(distance);
        }

        #endregion
    }
}
