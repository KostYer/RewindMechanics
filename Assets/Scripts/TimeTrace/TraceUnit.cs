using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RewindSystem
{
    public class TraceUnit: IDisposable
    { 
        public GameObject GameObject { get; }
        private MeshRenderer _renderer;
        private Material[] _materials;

        private bool _disposed;

        public TraceUnit(GameObject gameObject)
        {
            GameObject = gameObject;
            _renderer = gameObject.GetComponent<MeshRenderer>();
            _materials = _renderer.materials;
        }

        public async UniTask FadeOutAsync(float duration, float startingAlpha)
        {
            float time = 0f;
            while (time < duration)
            {
                float alpha = startingAlpha - (time / duration);
                alpha = Mathf.Clamp01(alpha);
                SetAlpha(alpha);
                time += Time.deltaTime;
                await UniTask.Yield(); // yield to next frame
            }

            SetAlpha(0f);
            Dispose();
        }

        private void SetAlpha(float alpha)
        {
            if (_materials == null) return;
            foreach (var mat in _materials)
            {
                mat.SetFloat("_Alpha", alpha);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            if (Application.isPlaying)
                Object.Destroy(GameObject);
            else
                Object.DestroyImmediate(GameObject);
        }
        
    }
}