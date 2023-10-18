using System.Threading.Tasks;
using DG.Tweening;
using SimpleMatch3.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace SimpleMatch3.Tile
{
    public class Tile : MonoBehaviour
    {
        public static float TileSize;
        public TileData Data;

        [SerializeField] private bool isBusy;
        public bool IsBusy => isBusy;

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
        }

        public void PlaySwipeAnim(Vector2Int toDirection, Tile toTile, UnityAction onComplete)
        {
            SetBusy(true);
            var toPosition = (transform.position + toTile.transform.position) / 2;
            Data.CurrentDrop.PlaySwipeAnim(toDirection, transform.position, toPosition, onComplete);
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
            
            SetBusy(false);
            
            Data.CurrentDrop.Explode();
        }
    }
}