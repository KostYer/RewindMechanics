using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Snapshots;
using StarterAssets.Interfaces;
using UnityEngine;

namespace RewindSystem
{
    public class TransformRecorder: IRecorder<TransformSnapshot>
    {
        private List<TransformSnapshot> _snapshots = new List<TransformSnapshot>();
        private CancellationTokenSource _tokenSource;
        private Transform _targetTransform;
        
        public List<TransformSnapshot> Snapshots => _snapshots;
        public float MaxDuration { get; private set; } 

        public TransformRecorder(Transform target, float maxDur)
        {
            _targetTransform = target;
            MaxDuration = maxDur;
        }


        public void StartRecording()
        {
            Clear();
            _tokenSource?.Dispose();
            _tokenSource = new CancellationTokenSource();
            RecordSnapshots(_tokenSource.Token);
        }

        public void StopRecording()
        {
            _tokenSource?.Cancel();  
            _tokenSource?.Dispose();
            _tokenSource = null;
        }

        public void Clear()
        {
            _snapshots.Clear();
        }

        public List<TransformSnapshot> GetSnapshots()
        {
            return _snapshots;
        }
        
        private async UniTaskVoid RecordSnapshots(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                float timeNow = Time.time;

                _snapshots.Add(new TransformSnapshot {
                    time = timeNow,
                    position = _targetTransform.position,
                    rotation = _targetTransform.rotation
                });
                while (_snapshots.Count > 0 && timeNow - _snapshots[0].time > MaxDuration)
                    _snapshots.RemoveAt(0);
                
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
            }
        }
    }
}