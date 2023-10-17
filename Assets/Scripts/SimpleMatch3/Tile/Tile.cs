using System.Collections.Generic;
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
        private Dictionary<Vector2Int, Sequence> _swipeSequences;
        private Sequence _swipeSequence;

        private void Awake()
        {
            var swipeDirections = new List<Vector2Int>()
            {
                Vector2Int.left,
                Vector2Int.right,
                Vector2Int.up,
                Vector2Int.down
            };

            
            
            // _swipeSequences = new Dictionary<Vector2Int, Sequence>();
            //
            // foreach (var direction in swipeDirections)
            // {
            //     var swipeSequence  = DOTween.Sequence();
            //     var offset = (Vector3) Vector2.Perpendicular(direction);
            //     swipeSequence.Join(Data.CurrentDrop.transform.DOPath(new[]
            //     {
            //         (transform.position + direction.ToVec3() * TileSize) / 2 + offset * 0.35f,
            //         transform.position
            //     }, 0.2f, PathType.CatmullRom));
            //     swipeSequence.Join(Data.CurrentDrop.transform.DOPunchScale(Vector3.one * 0.35f, 0.2f));
            //
            //     _swipeSequences.Add(direction, swipeSequence);
            // }
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
        }

        public void PlaySwipeAnim(Vector2Int toDirection, Tile toTile, UnityAction onComplete)
        {
            SetBusy(true);
            
            var swipeSequence = DOTween.Sequence();
            var offset = (Vector3) Vector2.Perpendicular(toDirection);
            swipeSequence.Join(Data.CurrentDrop.transform.DOPath(new[]
            {
                (transform.position + toTile.transform.position) / 2 + offset * 0.35f,
                transform.position
            }, 0.2f, PathType.CatmullRom));
            
            swipeSequence.Join(Data.CurrentDrop.transform.DOPunchScale(Vector3.one * 0.35f, 0.2f));
            
            swipeSequence.OnComplete(() => onComplete?.Invoke());
            swipeSequence.Play();
        }
        
        public void PlaySwipeWithoutExplosionAnim(Vector2Int toDirection, Tile toTile, UnityAction halfwayCallback)
        {
            var firstPos = transform.position;
            var finalPos = toTile.transform.position;
            var drop = Data.CurrentDrop;
            var offset = (Vector3) Vector2.Perpendicular(toDirection);
             
            SetBusy(true);
            Sequence sequence = DOTween.Sequence();
            sequence.Join(drop.transform.DOPath(new[]
            {
                (finalPos + firstPos) / 2 + offset * 0.35f,
                finalPos
            }, 0.2f, PathType.CatmullRom).OnWaypointChange(waypoint =>
            {
                if(waypoint == 1)
                    halfwayCallback?.Invoke();
            }).OnComplete(() =>
            {
                drop.transform.DOPath(new[]
                {
                    (finalPos + firstPos) / 2 - offset * 0.35f,
                    firstPos
                }, 0.2f, PathType.CatmullRom).OnWaypointChange((waypoint =>
                {
                    if(waypoint == 1)
                        halfwayCallback?.Invoke();
                })).OnComplete(() => SetBusy(false));
            }));

            sequence.Join(drop.transform.DOPunchScale(Vector3.one * 0.35f, 0.2f).OnComplete(() =>
            {
                drop.transform.DOPunchScale(Vector3.one * 0.35f, 0.2f);
            }));
            
            sequence.Play();
        }

        public void SetBusy(bool busy)
        {
            isBusy = busy;
        }

        public void Explode()
        {
            if(!IsEmpty())
                Destroy(Data.CurrentDrop.gameObject);
        }
    }
}