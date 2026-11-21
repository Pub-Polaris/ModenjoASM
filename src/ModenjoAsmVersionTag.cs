using System;
using TMPro;
using UnityEngine;

namespace ModenjoASM
{
    /// <summary>
    /// Marks the Aotenjo build as modded in the visible version label.
    /// It scans runtime UI text instead of touching a prefab or a scene file.
    /// </summary>
    public sealed class ModenjoAsmVersionTag : MonoBehaviour
    {
        private const string Tag = "Modded!! By-ModenjoASM";
        private float _nextScan;

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Update()
        {
            if (Time.unscaledTime < _nextScan)
            {
                return;
            }

            _nextScan = Time.unscaledTime + 1.0f;
            string version = Aotenjo.Utils.GetConfigValue("version");
            if (string.IsNullOrEmpty(version) || version == "ERROR")
            {
                return;
            }

            foreach (var text in FindObjectsOfType<TextMeshProUGUI>())
            {
                if (text == null || string.IsNullOrEmpty(text.text) || text.text.Contains(Tag))
                {
                    continue;
                }

                // The actual version label is short; skip descriptions that merely
                // mention the game version.
                if (text.text.Length <= 32 && text.text.Contains(version))
                {
                    text.text = text.text + "\n" + Tag;
                }
            }
        }
    }
}
