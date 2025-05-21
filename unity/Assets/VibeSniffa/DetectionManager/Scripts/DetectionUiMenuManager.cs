// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PassthroughCamera;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace VibeSniffa
{
    public class DetectionUiMenuManager : MonoBehaviour
    {
        [Header("Ui elements ref.")]
        [SerializeField] private GameObject m_initialPanel;
        [SerializeField] private GameObject m_noPermissionPanel;
        [SerializeField] private Text m_labelInfromation;
        [SerializeField] private AudioSource m_buttonSound;

        public bool IsInputActive { get; set; } = false;

        public UnityEvent<bool> OnPause;

        private bool m_initialMenu;

        // pause menu
        public bool IsPaused { get; private set; } = true;

        #region Unity Functions
        private IEnumerator Start()
        {
            m_noPermissionPanel.SetActive(false);
            m_cnt = 0;
            m_debug_delay = 0;
            s_debugstr = "init...";
            UpdateDebugInfo();

            while (!PassthroughCameraPermissions.HasCameraPermission.HasValue)
            {
                yield return null;
            }
            m_initialPanel.SetActive(false);
            if (PassthroughCameraPermissions.HasCameraPermission == false)
            {
                OnNoPermissionMenu();
                s_debugstr = "init failed.";
                UpdateDebugInfo();
            }
            else
            {
                s_debugstr = "init done.";
                UpdateDebugInfo();
            }
        }

        private void Update()
        {
            if (m_debug_delay <= 0)
            {
                if (s_debugQueue.Count > 0) s_debugstr = s_debugQueue.Dequeue();
                m_debug_delay = DEBUG_DELAY_S;
            }
            else
            {
                m_debug_delay -= Time.deltaTime;
            }

            UpdateDebugInfo();
        }
        #endregion

        #region Ui state: No permissions Menu
        private void OnNoPermissionMenu()
        {
            m_initialMenu = false;
            IsPaused = true;
            m_initialPanel.SetActive(false);
            m_noPermissionPanel.SetActive(true);
        }
        #endregion

        #region Ui state: Debugging
        private int m_cnt;
        private float m_debug_delay;
        private static string s_debugstr;
        private const float DEBUG_DELAY_S = 1f;
        private const int BLOCKSIZE = 40;
        private static Queue<string> s_debugQueue = new();

        public void UpdateCounter() => m_cnt++;

        public static void AddDebugMsg(string debugstr)
        {
            if (debugstr.Length < BLOCKSIZE)
                s_debugQueue.Enqueue(debugstr);
            else
            {
                foreach (
                    var str in Enumerable.Range(0, (debugstr.Length + BLOCKSIZE - 1) / BLOCKSIZE)
                         .Select(i => debugstr.Substring(i * BLOCKSIZE, Math.Min(BLOCKSIZE, debugstr.Length - i * BLOCKSIZE)))
                         .ToList()
                )
                    s_debugQueue.Enqueue(str);
            }
        }

        private void UpdateDebugInfo()
        {
            m_labelInfromation.text = $"VibeSniffa Alpha\nAI model: Yolo Face Detection + Vision Transformer Classifier\nClicked {m_cnt}\nDebug: {s_debugstr}";
        }
        #endregion

        #region Ui state: Initial Menu
        public void OnInitialMenu()
        {
            // Check if we have the Scene data permission
            m_initialMenu = true;
            IsPaused = true;
            m_initialPanel.SetActive(true);
            m_noPermissionPanel.SetActive(false);
        }

        public void CloseInitialMenu()
        {
            if (m_initialMenu)
            {
                m_buttonSound?.Play();

                m_initialMenu = false;
                IsPaused = false;

                m_initialPanel.SetActive(false);
                m_noPermissionPanel.SetActive(false);

                OnPause?.Invoke(false);
            }
        }
        #endregion
    }
}
