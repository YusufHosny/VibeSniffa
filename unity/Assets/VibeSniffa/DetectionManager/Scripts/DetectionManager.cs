// Copyright (c) Meta Platforms, Inc. and affiliates.

using PassthroughCamera;
using Unity.Mathematics;
using UnityEngine;

namespace VibeSniffa
{
    public class DetectionManager : MonoBehaviour
    {
        [SerializeField] private WebCamTextureManager m_webCamTextureManager;

        [Header("Ui references")]
        [SerializeField] private DetectionUiMenuManager m_uiMenuManager;

        [Header("Placement configureation")]
        [SerializeField] private GameObject m_spwanMarker;
        [SerializeField] private float m_spawnDistance = 0.25f;
        [SerializeField] private AudioSource m_placeSound;

        [Header("Inference ref")]
        [SerializeField] private InferenceRunManager m_runInference;
        [SerializeField] private InferenceUiManager m_uiInference;
        [Space(10)]
        private bool m_isStarted = false;
        private float m_inference_cooldown;
        private const float MAX_COOLDOWN_S = 2;

        #region Unity Functions
        private void Start()
        {
            m_inference_cooldown = MAX_COOLDOWN_S;
            DetectionUiMenuManager.AddDebugMsg($"Started.");
        }


        private void Update()
        {
            // Get the WebCamTexture CPU image
            var hasWebCamTextureData = m_webCamTextureManager.WebCamTexture != null;

            if (!m_isStarted)
            {
                // Manage the Initial Ui Menu
                if (hasWebCamTextureData)
                {
                    m_uiMenuManager.OnInitialMenu();
                    m_isStarted = true;
                }
            }
            else
            {
                // Cooldown for inference
                m_inference_cooldown -= Time.deltaTime;

                if (hasWebCamTextureData && m_inference_cooldown <= 0)
                {
                    m_runInference.InferBounds(m_webCamTextureManager.WebCamTexture);
                    m_inference_cooldown = MAX_COOLDOWN_S;
                }
            }
        }
        #endregion

        #region Public Functions

        public void OnClickA()
        {
            m_uiMenuManager.UpdateCounter();
            m_uiMenuManager.CloseInitialMenu();

            // get box closest to center
            if (m_uiInference.BoxDrawn.Count == 0) return;

            var closestBox = m_uiInference.BoxDrawn[0];
            var minDist = float.MaxValue;

            var intrinsics = PassthroughCameraUtils.GetCameraIntrinsics(m_uiInference.CameraEye);
            var camRes = intrinsics.Resolution;
            var centerPixel = new Vector2(camRes.x / 2, camRes.y / 2);
            foreach (var box in m_uiInference.BoxDrawn)
            {
                var boxToCenter = Vector2.Distance(centerPixel, Camera.main.WorldToScreenPoint(box.WorldPos));
                if (boxToCenter < minDist)
                {
                    closestBox = box;
                    minDist = boxToCenter;
                }
            }

            // use closest box to infer emotion
            m_runInference.InferEmotion(closestBox, closestBox.DirtyBit);
        }
        #endregion
    }
}
