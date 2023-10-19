using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleMatch3.UI
{
    public class FPSButtonToggle : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text fpsText;

        private void Awake()
        {
            button.onClick.AddListener(ToggleText);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveAllListeners();
        }

        private void ToggleText()
        {
            fpsText.gameObject.SetActive(!fpsText.gameObject.activeSelf);
        }
    }
}