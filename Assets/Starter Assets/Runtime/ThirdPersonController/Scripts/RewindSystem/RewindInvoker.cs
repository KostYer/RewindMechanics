using System.Threading;
using Cysharp.Threading.Tasks;
using StarterAssets.ScriptableObjects;
using StarterAssets.Utilities;
using UnityEngine;

namespace RewindSystem
{
    public class RewindInvoker: MonoSingleton<RewindInvoker>
    {
        [SerializeField] private RewindEventChannelSO _eventChannel;
        [SerializeField] private RewindSettingsSO _rewindSettings;

        private float _rewindElapsed = 0f;
        private float _rewindStartTime = 0f;
        
        private bool _isRewinding;
        
        private CancellationTokenSource _rewindCTS;

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
            
            RewindRoutineAsync(_rewindCTS.Token).Forget();

            _eventChannel.RaiseRewindStarted();
                
            _isRewinding = true;
        }

        private void StopRewind()
        {
            if (!_isRewinding) return;
            
            _rewindCTS?.Cancel();
            _rewindCTS?.Dispose();
            _rewindCTS = null;
            
            _eventChannel.RaiseRewindEnded();
            _isRewinding = false;
        }

        private async UniTask RewindRoutineAsync(CancellationToken token)
        {
            _rewindStartTime = Time.time;
            _rewindElapsed = 0f;

                while (!token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                  

                    _rewindElapsed += Time.unscaledDeltaTime * _rewindSettings.RewindSpeed;
                    float targetTime = _rewindStartTime - _rewindElapsed;

                    if (targetTime <= 0f)
                    {
                        Debug.Log($"[targetTime <= 0]");
                        StopRewind();
                        break;
                    }

                    _eventChannel.RaiseRewindTick(targetTime);
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
        }
    }
}