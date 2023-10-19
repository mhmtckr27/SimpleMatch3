using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace SimpleMatch3.Pool
{
    public class PooledVFX : MonoBehaviour
    {
        [SerializeField] private ParticleSystem vfx;

        private ObjectPool<PooledVFX> _parentPool;

        public void SetParentPool(ObjectPool<PooledVFX> parentPool)
        {
            _parentPool = parentPool;
        }
        
        public void Play(Vector3 position)
        {
            transform.position = position;
            StartCoroutine(PlayCor());
        }

        private IEnumerator PlayCor()
        {
            vfx.Play();

            while (vfx.isPlaying)
                yield return null;

            _parentPool.Release(this);
        }
    }
}