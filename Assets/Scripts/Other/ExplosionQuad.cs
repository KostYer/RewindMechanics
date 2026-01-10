using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Other
{
    public class ExplosionQuad: MonoBehaviour
    {
        [SerializeField] private bool _isEnabled;
        [SerializeField] private float _lifetime = .5f;
        [SerializeField] private float _lifeSpanDebug = 20f;
        [Range(0, 1f)][SerializeField] private float _alpha = 1f;
        [SerializeField] private AnimationCurve _fadeCurve;
      
        private string _alphaProperty = "_EffectFade";
        private string _animationProperty = "_Animation";
        
        private Renderer _renderer;
        private MaterialPropertyBlock _mpb;
        private int _alphaId;
        private int _animId;
        
        private Camera _camera;

        private float _span;

        [SerializeField] private bool _isDebug;

        private float _offsetLen = 4f;
        
       // private float 

       private const float _ripplesAnimSpeed = 2f;

       private void Awake()
       {
           _alphaId = Shader.PropertyToID(_alphaProperty);
           _animId = Shader.PropertyToID(_animationProperty);
           _renderer = GetComponent<Renderer>();
           _mpb = new MaterialPropertyBlock();
       }
       
       private void Start()
       {
           Debug.Log($"[ExplosionQuad] Start");
           _span = _lifetime;
           _camera = Camera.main;
       }

       void LateUpdate()
       { 
           if(!_isEnabled) return;
           _span -= Time.deltaTime;
           if (_span <= 0f)
           {
               _isEnabled = false;
               return;
           }

           Vector3 direction = (_camera.transform.position - transform.position).normalized;
           transform.forward = -direction;
       }

       public void OnCollided()
        {
            Debug.Log($"[SetAlpha] _alphaId {_alphaId}");
            OffsetTowardsCamera();
            var cts = new CancellationTokenSource();

            SetRippleDirection(_ripplesAnimSpeed);
            PlayFadeAsync(_lifetime, false, cts.Token, TurnOffGameObject).Forget();
        }

        public void OnReverse()
        {
           
            Debug.Log($"[SetAlpha] _alphaId {_alphaId}");
            OffsetTowardsCamera();
            var cts = new CancellationTokenSource();

            SetRippleDirection(_ripplesAnimSpeed * -1f);
            PlayFadeAsync(_lifetime, true, cts.Token, TurnOffGameObject).Forget();
        }

        private void OffsetTowardsCamera()
        {
            Vector3 before = transform.position;

            Vector3 toCamera =
                (Camera.main.transform.position - transform.position).normalized;

            transform.position += toCamera * _offsetLen;

            
            Debug.Log($"[OffsetTowardsCamera] before {before}, after {transform.position}, len: {(Vector3.Distance(before, transform.position))}");
        }

        private async UniTask PlayFadeAsync(float lifetimeSeconds, bool isReverse, CancellationToken token, Action onComplete = null)
        {
            if (lifetimeSeconds <= 0f)
            {
                Debug.Log($"[PlayFadeAsync] lifetimeSeconds <= 0f");
                SetAlpha(0f);
                return;
            }

            float elapsed = 0f;

            // Set initial value immediately
            SetAlpha(_fadeCurve.Evaluate(isReverse ? 0f: 1f));

            while (elapsed < lifetimeSeconds)
            {
                token.ThrowIfCancellationRequested();

                float t01 = Mathf.Clamp01(elapsed / lifetimeSeconds);     // <-- projection into lifetime
                float alpha = Mathf.Clamp01(_fadeCurve.Evaluate(t01));   // curve is normalized 0..1 domain
                Debug.Log($"[PlayFadeAsync] alpha: {1 - alpha}");

                var endAlpha = isReverse ? alpha : 1 - alpha;
                SetAlpha(endAlpha);

             

                // wait next frame (Update-ish timing)
                await UniTask.Yield(PlayerLoopTiming.Update, token);

                elapsed += Time.deltaTime;
            }
         
            // Ensure we end exactly at 1.0 time
           SetAlpha(Mathf.Clamp01(_fadeCurve.Evaluate(isReverse ? 1f: 0f)));
           
           onComplete?.Invoke();
        }

        private void TurnOffGameObject()
        {
            gameObject.SetActive(false);
        }


        private void SetAlpha(float value01)
        {
           
            Debug.Log($"[SetAlpha] value01 {value01}");
            _alpha = Mathf.Clamp01(value01);

            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat(_alphaId, _alpha);
            _renderer.SetPropertyBlock(_mpb);
        }

        private void SetRippleDirection(float value)
        {
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat(_animId, value);
            _renderer.SetPropertyBlock(_mpb);
        }
    }
}