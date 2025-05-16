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
            m_debugstr = "init...";
            UpdateDebugInfo();

            while (!PassthroughCameraPermissions.HasCameraPermission.HasValue)
            {
                yield return null;
            }
            m_initialPanel.SetActive(false);
            if (PassthroughCameraPermissions.HasCameraPermission == false)
            {
                OnNoPermissionMenu();
                m_debugstr = "init failed.";
                UpdateDebugInfo();
            }
            else
            {
                m_debugstr = "init done.";
                UpdateDebugInfo();
            }
        }

        private void Update()
        {
            if (m_debug_delay <= 0)
            {
                if (m_debugQueue.Count > 0) m_debugstr = m_debugQueue.Dequeue();
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
        private string m_debugstr;
        private const float DEBUG_DELAY_S = 1f;
        private const int BLOCKSIZE = 40;
        private Queue<string> m_debugQueue = new();

        public void UpdateCounter() => m_cnt++;

        public void AddDebugMsg(string debugstr)
        {
            if (debugstr.Length < BLOCKSIZE)
                m_debugQueue.Enqueue(debugstr);
            else
            {
                foreach (
                    var str in Enumerable.Range(0, (debugstr.Length + BLOCKSIZE - 1) / BLOCKSIZE)
                         .Select(i => debugstr.Substring(i * BLOCKSIZE, Math.Min(BLOCKSIZE, debugstr.Length - i * BLOCKSIZE)))
                         .ToList()
                )
                    m_debugQueue.Enqueue(str);
            }
        }

        private void UpdateDebugInfo()
        {
            m_labelInfromation.text = $"VibeSniffa Alpha\nAI model: Yolo Face Detection + Vision Transformer Classifier\nClicked {m_cnt}\nDebug: {m_debugstr}";
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
