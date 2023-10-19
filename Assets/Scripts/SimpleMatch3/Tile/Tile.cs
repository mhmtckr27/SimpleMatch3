using System.Threading.Tasks;
using DG.Tweening;
using SimpleMatch3.EventInterfaces;
using SimpleMatch3.Extensions;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace SimpleMatch3.Tile
{
    public class Tile : MonoBehaviour
    {
        public static float TileSize;
        public TileData Data;
        [SerializeField] private AudioClip swipeNotAllowedClip;
        [SerializeField] private AudioClip swipeClip;
        [SerializeField] private bool isBusy;
        private SignalBus _signalBus;
        public bool IsBusy => isBusy;

        [Inject]
        private void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public Drop.Drop SetDrop(Drop.Drop newDrop)
        {
            var oldDrop = Data.CurrentDrop;
            Data.CurrentDrop = newDrop;
            
            if (newDrop)
                newDrop.CurrentTileCoords = Data.Coordinates;
            
            return oldDrop;
        }

        public bool IsEmpty() => Data.CurrentDrop == null;
        
        public void PlaySwipeNotAllowedAnim(Vector2Int toDirection)
        {
            SetBusy(true);
            Data.CurrentDrop.transform.DOPunchPosition(toDirection.ToVec3() * 0.2f, 0.2f).OnComplete(() => SetBusy(false));
            _signalBus.Fire(new IPlayAudio.OnPlayAudio()
            {
                ClipToPlay = swipeNotAllowedClip
            });
        }

        public void PlaySwipeAnim(Vector2Int toDirection, Tile toTile, UnityAction onComplete)
        {
            SetBusy(true);
            var toPosition = (transform.position + toTile.transform.position) / 2;
            Data.CurrentDrop.PlaySwipeAnim(toDirection, transform.position, toPosition, onComplete);
            _signalBus.Fire(new IPlayAudio.OnPlayAudio()
            {
                ClipToPlay = swipeClip
            });
        }

        public void SetBusy(bool busy)
        {
            isBusy = busy;
        }

        public async Task Explode()
        {
            if (IsEmpty())
                return;
            
            SetBusy(true);

            while (Data.CurrentDrop.IsFalling)
            {
                await Task.Delay(20);
            }
            
            await Data.CurrentDrop.Explode();
            
            SetBusy(false);
            SetDrop(null);
        }
    }
}