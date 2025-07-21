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
 
        private bool _isRewinding;
        
        private CancellationTokenSource _rewindCTS;

        private float rewindEndTime;

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
            if(_isRewinding) return;
            
            _rewindCTS?.Cancel();  
            _rewindCTS?.Dispose();
            _rewindCTS = new CancellationTokenSource();
            
            RewindRoutineAsync(_rewindCTS.Token).Forget();
            _isRewinding = true;
            _eventChannel.RaiseRewindStarted();
        }

        private void StopRewind()
        {
            Debug.Log($"[RewindInvoker] StopRewind attempted");
            if(!_isRewinding) return;
            Debug.Log($"[RewindInvoker] StopRewind invoked");
           
            _rewindCTS?.Cancel();
            _rewindCTS?.Dispose();
            _rewindCTS = null;

            _isRewinding = false;
            
            _eventChannel.RaiseRewindEnded();
            rewindEndTime = Time.time;
        }

    
        private async UniTask RewindRoutineAsync(CancellationToken token)
        {
            var rewindStartTime = Time.time;
            var rewindElapsed = 0f;
            var endTime =(rewindStartTime - rewindEndTime)/_rewindSettings.RewindSpeed;

                while (!token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                  
                    float step = Time.deltaTime * _rewindSettings.RewindSpeed;
                    rewindElapsed += step;
                    float targetTime = rewindStartTime - rewindElapsed; // for broadcasting


                    endTime -= step; //for exiting
                  
                    if (endTime <= 0f)
                    {
                        await UniTask.NextFrame();  
                        StopRewind();
                    } 

                    _eventChannel.RaiseRewindTick(targetTime);
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
        }
    }
}