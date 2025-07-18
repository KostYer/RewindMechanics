using System;
using System.Collections.Generic;
using Snapshots;
using UnityEngine;

namespace Recorders
{
    public class RigidbodyRecorder: MonoBehaviour
    {
        [SerializeField] private Rigidbody _rb;
        public bool IsAtive;
        private float _maxTime = 5f;
        List<RbSnapshot> snapshots = new List<RbSnapshot>();

        private RbSnapshot _current;

        public List<RbSnapshot> Snapshots => snapshots;
        float totalRecordedTime = 5f; // e.g. keep last 5 seconds

        
        void FixedUpdate() {
            
            if (!IsAtive) return;
            
            float timeNow = Time.time;

            snapshots.Add(new RbSnapshot {
                Time = timeNow,
                Position = transform.position,
                Rotation = transform.rotation,
                Velocity = _rb.velocity,
                AngularVelocity = _rb.angularVelocity
            });

            // Trim old entries
            while (snapshots.Count > 0 && timeNow - snapshots[0].Time > totalRecordedTime)
                snapshots.RemoveAt(0);
        }

        public void StartRecord()
        {
            snapshots.Clear();
            _rb.isKinematic = false;
            IsAtive = true;
        }
        
        public void StopRecord()
        {
            IsAtive = false;
            _rb.isKinematic = true;
        }
        
        
        public void ApplyTo(RbSnapshot snapshot)
        {
            _rb.position = snapshot.Position;
            _rb.rotation = snapshot.Rotation;
            _rb.velocity = snapshot.Velocity;
            _rb.angularVelocity = snapshot.AngularVelocity;
        }

       
        
        public RbSnapshot FindClosestSnapshot(float rewindTime)
        {
            if (snapshots.Count == 0)
                throw new InvalidOperationException("No snapshots recorded.");

            if (rewindTime <= snapshots[0].Time)
                return snapshots[0];

            if (rewindTime >= snapshots[^1].Time)
                return snapshots[^1];

            // Search from the end
            for (int i = snapshots.Count - 1; i > 0; i--)
            {
                var a = snapshots[i - 1];
                var b = snapshots[i];

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
    }
}