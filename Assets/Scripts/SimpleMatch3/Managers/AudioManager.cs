using System;
using System.Collections;
using System.Threading.Tasks;
using SimpleMatch3.EventInterfaces;
using UnityEngine;
using Zenject;

namespace SimpleMatch3.Managers
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioSource musicSource;

        private SignalBus _signalBus;

        [Inject]
        private void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
            
            _signalBus.Subscribe<IPlayAudio.OnPlayAudio>(OnPlayAudio);
            _signalBus.Subscribe<IMuteAudio.OnMuteAudio>(OnMuteAudio);
        }

        private void OnDestroy()
        {
            _signalBus.Unsubscribe<IPlayAudio.OnPlayAudio>(OnPlayAudio);
            _signalBus.Unsubscribe<IMuteAudio.OnMuteAudio>(OnMuteAudio);
        }

        private void OnMuteAudio(IMuteAudio.OnMuteAudio data)
        {
            audioSource.mute = data.Mute;
            musicSource.mute = data.Mute;
        }

        private void OnPlayAudio(IPlayAudio.OnPlayAudio data)
        {
            audioSource.PlayOneShot(data.ClipToPlay);
        }
    }
}