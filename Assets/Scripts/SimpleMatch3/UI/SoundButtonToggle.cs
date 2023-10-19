using System;
using SimpleMatch3.EventInterfaces;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SimpleMatch3.UI
{
    public class SoundButtonToggle : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private Sprite audioIcon;
        [SerializeField] private Sprite audioMutedIcon;
        
        private SignalBus _signalBus;
        private bool _isMuted = false;

        [Inject]
        private void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        private void Awake()
        {
            button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveAllListeners();
        }

        private void OnClick()
        {
            _isMuted = !_isMuted;
            _signalBus.Fire(new IMuteAudio.OnMuteAudio()
            {
                Mute = _isMuted
            });
            icon.sprite = _isMuted ? audioMutedIcon : audioIcon;
        }
    }
}