using System;
using DG.Tweening;
using SimpleMatch3.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SimpleMatch3.Tile
{
    public class Tile : MonoBehaviour
    {
        public TileData Data;

        [SerializeField] private bool isBusy;
        public bool IsBusy => isBusy;

        public Drop.Drop SetDrop(Drop.Drop newDrop)
        {
            var oldDrop = Data.CurrentDrop;
            Data.CurrentDrop = newDrop;
            return oldDrop;
        }

        public void PlaySwipeNotAllowedAnim(Vector2Int toDirection)
        {
            SetBusy(true);
            Data.CurrentDrop.transform.DOPunchPosition(toDirection.ToVec3() * 0.2f, 0.2f).OnComplete(() => SetBusy(false));
        }

        public void PlaySwipeWithoutExplosionAnim(Vector2Int toDirection, Tile toTile)
        {
            var finalPos = toTile.transform.position;
            var offset = (Vector3) Vector2.Perpendicular(toDirection);
             
            SetBusy(true);
            Sequence sequence = DOTween.Sequence();
            sequence.Join(Data.CurrentDrop.transform.DOPath(new[]
            {
                (finalPos + transform.position) / 2 + offset * 0.35f,
                finalPos
            }, 0.2f, PathType.CatmullRom).OnComplete(() =>
            {
                Data.CurrentDrop.transform.DOPath(new[]
                {
                    (finalPos + transform.position) / 2 - offset * 0.35f,
                    transform.position
                }, 0.2f, PathType.CatmullRom).OnComplete(() => SetBusy(false));
            }));

            sequence.Join(Data.CurrentDrop.transform.DOPunchScale(Vector3.one * 0.35f, 0.2f).OnComplete(() =>
            {
                Data.CurrentDrop.transform.DOPunchScale(Vector3.one * 0.35f, 0.2f);
            }));

            sequence.Play();
        }

        public void SetBusy(bool busy)
        {
            isBusy = busy;
        }
    }
}