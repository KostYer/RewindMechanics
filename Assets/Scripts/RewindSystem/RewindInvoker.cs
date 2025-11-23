using System.Threading;
using Cysharp.Threading.Tasks;
using RewindSystem.RigidbidyRewind;
using StarterAssets.ScriptableObjects;
using StarterAssets.Utilities;
using UnityEngine;

namespace RewindSystem
{
    public class RewindInvoker: MonoSingleton<RewindInvoker>
    {
        public float RewindElapsed => _rewindElapsed;
        [SerializeField] private RewindEventChannelSO _eventChannel;
        [SerializeField] private RewindSettingsSO _rewindSettings;

        [SerializeField] private bool _sampleCureve = true;
        [SerializeField] private AnimationCurve _curve;
        
        [SerializeField] private RbRewinder _rbRewinderDebug;
        private float startTime;
        private float endTime;
        
        
        private bool _isRewinding;
        
        private CancellationTokenSource _rewindCTS;

        private float rewindEndTime;
        private float _rewindElapsed;

        private void Start()
        {
            rewindEndTime = Time.time;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                StartRewind();
            }
            
            else if (Input.GetKeyUp(KeyCode.R))
            {
                StopRewind();
            }
        }

        private void StartRewind()
        {
            if (_isRewinding) return;

            _rewindCTS?.Cancel();
            _rewindCTS?.Dispose();
            _rewindCTS = new CancellationTokenSource();

            _isRewinding = true;

            // Tell all RbRewinders to stop recording (they’ll freeze their lists)
            _eventChannel.RaiseRewindStarted();

            // Use only the debug rewinder *for now*
            if (!_rbRewinderDebug.HasSnapshots)
            {
                Debug.LogWarning("[RewindInvoker] No snapshots on debug rewinder.");
                _isRewinding = false;
                return;
            }

            // IMPORTANT: directly use recorder’s own range
            float startTime = _rbRewinderDebug.FirstSnapshotTime;
            float endTime   = _rbRewinderDebug.LastSnapshotTime;

            _rewindElapsed = 0f;

            RewindRoutineAsync(startTime, endTime, _rewindCTS.Token).Forget();
        }

        private void StopRewind()
        {
            if(!_isRewinding) return;
           
            _rewindCTS?.Cancel();
            _rewindCTS?.Dispose();
            _rewindCTS = null;

            _isRewinding = false;
            
            _eventChannel.RaiseRewindEnded();
            rewindEndTime = Time.time;
        }

    
        private async UniTask RewindRoutineAsync(float startTime, float endTime, CancellationToken token)
        {
            float currentTime = endTime;
            _rewindElapsed = 0f;

            // total "recorded" span we’re rewinding through
            float recordedDuration = Mathf.Max(0.0001f, endTime - startTime);

            while (!token.IsCancellationRequested && currentTime > startTime)
            {
                token.ThrowIfCancellationRequested();

                // 0 at start of rewind, 1 at the end of rewind
                float normalized = Mathf.Clamp01(_rewindElapsed / recordedDuration);

                // sample curve or just use 1
                float curveMultiplier = 1f;
                if (_sampleCureve && _curve != null)
                {
                    curveMultiplier = _curve.Evaluate(normalized);
                }

                // base step in recorded-time space, modulated by curve
                float stepRecordedTime = Time.fixedDeltaTime * _rewindSettings.RewindSpeed * curveMultiplier;

                _rewindElapsed += stepRecordedTime;
                currentTime -= stepRecordedTime;

                if (currentTime < startTime)
                    currentTime = startTime;

                _eventChannel.RaiseRewindTick(currentTime);

                if (currentTime <= startTime)
                {
                    StopRewind();
                    break;
                }

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
            }
        }

        
        
         
    }
}