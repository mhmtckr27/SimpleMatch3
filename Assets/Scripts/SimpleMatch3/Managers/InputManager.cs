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

        [SerializeField] private bool canSwipe;
        
        private Vector3 _mouseDownPosition;
        private Vector3 _currentMousePosition;
        private Tile.Tile _mouseDownTile;
        private Vector2Int _swipeDirection = Vector2Int.zero;
        private SignalBus _signalBus;
        private RaycastHit2D _hit;
        private Camera _mainCamera;
        private Vector3 _swipeVector;
        private Vector3 _swipeVectorAbs;
        private float _biggerSwipeAxis;
        private readonly ISwiped.OnSwiped _onSwiped = new();

        [Inject]
        private void Construct(SignalBus signalBus, Camera mainCamera)
        {
            _signalBus = signalBus;
            _mainCamera = mainCamera;
        }
        
        private void Awake()
        {
            canSwipe = true;
        }

        private void Update()
        {
            //must wait for mouse button up to accept input again.
            if (Input.GetMouseButtonUp(0))
                canSwipe = true;
            
            if(!canSwipe)
                return;
            
            //mouse button down, cache the tile under mouse.
            if (Input.GetMouseButtonDown(0))
            {
                _mouseDownPosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
                _hit = Physics2D.Raycast(_mouseDownPosition, Vector2.zero, 1f,
                    tileLayer);

                _mouseDownTile = _hit.collider ? _hit.collider.GetComponent<Tile.Tile>() : null;
            }
            else if (Input.GetMouseButton(0))
            {
                _currentMousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
                _swipeVector = _currentMousePosition - _mouseDownPosition;
                _swipeVectorAbs = _swipeVector.Abs();
                _biggerSwipeAxis = Mathf.Max(_swipeVectorAbs.x, _swipeVectorAbs.y);
                
                if(_biggerSwipeAxis < swipeThreshold)
                    return;

                if(_mouseDownTile == null)
                    return;
                
                //Swiped!
                _swipeDirection = Vector2.ClampMagnitude(_swipeVector.SelectBiggerAxis(), swipeThreshold).CeilToVec2Int();
                
                _onSwiped.SwipeDirection = _swipeDirection;
                _onSwiped.InputDownTileCoords = _mouseDownTile.Data.Coordinates;
                
                _signalBus.TryFire(_onSwiped);
                
                canSwipe = false;
            }
        }
    }
}