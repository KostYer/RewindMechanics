using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Snapshots;
using StarterAssets.Interfaces;
using UnityEngine;

namespace Recorders
{
    public class RigidbodyRecorder: IRecorder<RbSnapshot>
    {
        public float MaxDuration { get; set; }
        private Rigidbody _rb;
        private List<RbSnapshot> _snapshots = new List<RbSnapshot>();
      
        private CancellationTokenSource _tokenSource;

        public RigidbodyRecorder(Rigidbody rb, float maxDuration)
        {
            _rb = rb;
            MaxDuration = maxDuration;
        }

        public void StartRecording()
        {
            Clear();
            _rb.isKinematic = false;
            
            _tokenSource?.Cancel();  
            _tokenSource?.Dispose();
            _tokenSource = new CancellationTokenSource();

            RecordSnapshots(_tokenSource.Token);
        }
        
        public void StopRecording()
        {
            _rb.isKinematic = true;
            
            _tokenSource?.Cancel();  
            _tokenSource?.Dispose();
            _tokenSource = null;
        }
        
        public void Clear()
        {
            _snapshots.Clear(); 
        }

        public List<RbSnapshot> GetSnapshots()
        {
            return _snapshots;
        }
        
        public RbSnapshot FindClosestSnapshot(float rewindTime)
        {
            if (_snapshots.Count == 0)
                throw new InvalidOperationException("No snapshots recorded.");

            if (rewindTime <= _snapshots[0].Time)
                return _snapshots[0];

            if (rewindTime >= _snapshots[^1].Time)
                return _snapshots[^1];

            // Search from the end
            for (int i = _snapshots.Count - 1; i > 0; i--)
            {
                var a = _snapshots[i - 1];
                var b = _snapshots[i];

                if (rewindTime >= a.Time && rewindTime <= b.Time)
                {
                    float t = Mathf.InverseLerp(a.Time, b.Time, rewindTime);
                    return new RbSnapshot
                    {
                        Time = rewindTime,
                        Position = Vector3.Lerp(a.Position, b.Position, t),
                        Rotation = Quaternion.Slerp(a.Rotation, b.Rotation, t),
                        Velocity = Vector3.Lerp(a.Velocity, b.Velocity, t),
                        AngularVelocity = Vector3.Lerp(a.AngularVelocity, b.AngularVelocity, t)
                    };
                }
            }

            throw new InvalidOperationException("Time not within snapshot range.");
        }
        
        private async UniTaskVoid RecordSnapshots(CancellationToken token)
        {
            float totalRecordedTime = 5f; // Or however long you want to record

            while (!token.IsCancellationRequested)
            {
                float timeNow = Time.time;

                _snapshots.Add(new RbSnapshot {
                    Time = timeNow,
                    Position = _rb.position,
                    Rotation = _rb.rotation,
                    Velocity = _rb.velocity,
                    AngularVelocity = _rb.angularVelocity
                });

                // Trim old entries
                while (_snapshots.Count > 0 && timeNow - _snapshots[0].Time > totalRecordedTime)
                    _snapshots.RemoveAt(0);

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
            }
        }
    }
}