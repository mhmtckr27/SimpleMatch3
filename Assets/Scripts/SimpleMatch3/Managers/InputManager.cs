using System;
using SimpleMatch3.EventInterfaces;
using SimpleMatch3.Extensions;
using UnityEngine;
using Zenject;

namespace SimpleMatch3.Managers
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private LayerMask tileLayer;
        [SerializeField] private float swipeThreshold = 1f;

        [SerializeField]private bool _canSwipe;
        private Vector3 _mouseDownPosition;
        private Vector3 _currentMousePosition;
        private Tile.Tile _mouseDownTile;
        private Vector2Int _swipeDirection = Vector2Int.zero;
        private SignalBus _signalBus;

        [Inject]
        private void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }
        
        private void Awake()
        {
            _canSwipe = true;
        }

        private void Update()
        {
            if (Input.GetMouseButtonUp(0))
                _canSwipe = true;
            
            if(!_canSwipe)
                return;
            
            if (Input.GetMouseButtonDown(0))
            {
                _mouseDownPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var hit = Physics2D.Raycast(_mouseDownPosition, Vector2.zero, 1f,
                    tileLayer);

                if (hit.collider)
                    _mouseDownTile = hit.collider.GetComponent<Tile.Tile>();
            }
            else if (Input.GetMouseButton(0))
            {
                _currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var diff = _currentMousePosition - _mouseDownPosition;
                var diffAbs = diff.Abs();
                var bigger = Mathf.Max(diffAbs.x, diffAbs.y);
                
                if(bigger < swipeThreshold)
                    return;

                //Swiped!
                _swipeDirection = Vector2.ClampMagnitude(diff.SelectBiggerAxis(), swipeThreshold).CeilToVec2Int();

                _signalBus.TryFire(new ISwiped.OnSwiped()
                {
                    InputDownTileCoords = _mouseDownTile.Data.Coordinates,
                    SwipeDirection = _swipeDirection
                });
                
                // Debug.LogError("Swipe Direction : " + _swipeDirection);
                _canSwipe = false;
            }
        }
    }
}