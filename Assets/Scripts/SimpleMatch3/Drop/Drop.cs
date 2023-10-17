using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using SimpleMatch3.Extensions;
using UnityEngine;

namespace SimpleMatch3.Drop
{
    public class Drop : MonoBehaviour
    {
        public DropColor Color;
        public Vector2Int CurrentTileCoords;

        private float _defaultScale;
        private float _scaleModifier;
        private float _speed;
        private bool _isExploded;
        private CancellationTokenSource _tokenSource;

        public bool IsFalling { get; private set; }

        private void Awake()
        {
            _defaultScale = transform.localScale.x;
            ResetSpeed();
            
            _tokenSource = new CancellationTokenSource();
            _tokenSource.Token.ThrowIfCancellationRequested();
        }

        private void OnEnable()
        {
            _isExploded = false;
        }

        private void OnDisable()
        {
            _isExploded = true;
        }

        public Drop(DropColor color)
        {
            Color = color;
        }

        public void SetFalling(bool falling)
        {
            IsFalling = falling;
        }

        public void ResetSpeed()
        {
            _speed = Time.deltaTime;
        }

        public async Task DropTo(Vector3 targetPosition)
        {
            while (true)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, _speed);
                await Task.Yield();
                _speed = Mathf.Clamp(_speed + Time.deltaTime * 1f, 0, 0.3f);
                _scaleModifier = _speed;
                //TODO: null ref, drop null geliyor

                transform.localScale = new Vector3(_defaultScale - _scaleModifier, _defaultScale + _scaleModifier,
                    _defaultScale);

                if (transform.position.Approximately(targetPosition))
                    return;
            }
        }

        public async Task SquashAndStretch()
        {
            SetFalling(false);
            var elapsedTime = 0f;
            var totalTime = 0.1f;
            var targetScale = Vector3.one * _defaultScale;
            var startScale = transform.localScale;
            _scaleModifier /= 3;
                
            targetScale = new Vector3(_defaultScale + _scaleModifier, _defaultScale - _scaleModifier, 0);
            while (elapsedTime < totalTime)
            {
                transform.localScale = Vector3.MoveTowards(startScale, targetScale, elapsedTime / totalTime);
                await Task.Yield();
                elapsedTime += Time.deltaTime;
            }
                
            elapsedTime = 0;
            startScale = transform.localScale;
            targetScale = Vector3.one * _defaultScale;
            while (elapsedTime < totalTime)
            {
                transform.localScale = Vector3.MoveTowards(startScale, targetScale, elapsedTime / totalTime);
                await Task.Yield();
                elapsedTime += Time.deltaTime;
            }

            transform.localScale = targetScale;
        }
    }
}