using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using SimpleMatch3.EventInterfaces;
using SimpleMatch3.Extensions;
using SimpleMatch3.Pool;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using Zenject;

namespace SimpleMatch3.Drop
{
    public class Drop : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private AudioClip explodeClip;
        
        public DropColor Color;
        public Vector2Int CurrentTileCoords;

        private float _defaultScale;
        private float _scaleModifier;
        private float _speed;
        public bool IsExploded { get; private set; }
        private Coroutine _dropCor;
        private Coroutine _squashAndStretchCor;

        public bool IsFalling { get; private set; }
        private ObjectPool<Drop> _parentPool;
        private ObjectPool<PooledVFX> _vfxPool;
        private SignalBus _signalBus;

        [Inject]
        private void Construct(VFXPools vfxPools, SignalBus signalBus)
        {
            _signalBus = signalBus;
            
            _vfxPool = vfxPools.GetPool(Color);
        }
        
        private void Awake()
        {
            _defaultScale = transform.localScale.x;
            ResetSpeed();
        }

        private void OnEnable()
        {
            IsExploded = false;
            spriteRenderer.enabled = true;
        }

        public void SetParentPool(ObjectPool<Drop> parentPool)
        {
            _parentPool = parentPool;
        }
        
        public async Task Explode()
        {
            IsExploded = true;
            
            if (_dropCor != null)
                StopCoroutine(_dropCor);
            
            if (_squashAndStretchCor != null)
                StopCoroutine(_squashAndStretchCor);

            var canContinue = false;
            
            transform.DOScale(Vector3.one * 1.25f, 0.12f).OnComplete(() => canContinue = true);

            while (!canContinue)
                await Task.Yield();

            spriteRenderer.enabled = false;
            transform.localScale = Vector3.one * _defaultScale;
            _vfxPool.Get().Play(transform.position);
            _signalBus.Fire(new IPlayAudio.OnPlayAudio()
            {
                ClipToPlay = explodeClip
            });
            await Task.Delay(150);
            _parentPool.Release(this);
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
            _speed = 0;
        }

        public void PlaySwipeAnim(Vector2Int toDirection, Vector3 fromPosition, Vector3 toPosition, UnityAction onComplete)
        {
            var swipeSequence = DOTween.Sequence();
            var offset = (Vector3) Vector2.Perpendicular(toDirection);
            swipeSequence.Join(transform.DOPath(new[]
            {
                toPosition + offset * 0.35f,
                fromPosition
            }, 0.2f, PathType.CatmullRom));
            
            swipeSequence.Join(transform.DOPunchScale(Vector3.one * 0.35f, 0.2f));
            
            swipeSequence.OnComplete(() => onComplete?.Invoke());
            swipeSequence.Play();
        }
        
        public IEnumerator DropTo(Vector3 targetPosition)
        {
            if(IsExploded)
                yield break;
            _dropCor = StartCoroutine(DropTo_Internal(targetPosition));
            yield return _dropCor;
        }

        private IEnumerator DropTo_Internal(Vector3 targetPosition)
        {
            while (true)
            {
                yield return null;
                _speed = Mathf.Clamp(_speed + Time.deltaTime * 1.4f, 0, 0.3f);
                _scaleModifier = _speed * 0.6f;
                
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, _speed);
                
                transform.localScale = new Vector3(_defaultScale - _scaleModifier, _defaultScale + _scaleModifier,
                    _defaultScale);

                if (transform.position.Approximately(targetPosition))
                    yield break;
            }
        }

        public IEnumerator SquashAndStretch()
        {
            _squashAndStretchCor = StartCoroutine(SquashAndStretch_Internal());
            yield return _squashAndStretchCor;
        }

        private IEnumerator SquashAndStretch_Internal()
        {
            var elapsedTime = 0f;
            var totalTime = 0.1f;
            var targetScale = Vector3.one * _defaultScale;
            var startScale = transform.localScale;
            _scaleModifier /= 3;
                
            targetScale = new Vector3(_defaultScale + _scaleModifier, _defaultScale - _scaleModifier, 0);
            while (elapsedTime < totalTime)
            {
                transform.localScale = Vector3.MoveTowards(startScale, targetScale, elapsedTime / totalTime);
                yield return null;
                elapsedTime += Time.deltaTime;
            }
            SetFalling(false);
                
            elapsedTime = 0;
            startScale = transform.localScale;
            targetScale = Vector3.one * _defaultScale;
            while (elapsedTime < totalTime)
            {
                transform.localScale = Vector3.MoveTowards(startScale, targetScale, elapsedTime / totalTime);
                yield return null;
                elapsedTime += Time.deltaTime;
            }
            
            transform.localScale = targetScale;
        }
    }
}