using System.Collections.Generic;
using Snapshots;
using StarterAssets.Interfaces;
using UnityEngine;

namespace RewindSystem
{
    public class TransformRecorder: MonoBehaviour,IRecorder<TransformSnapshot>
    {
        public bool IsReversing;
        private float _maxTime = 5f;
        private List<TransformSnapshot> _snapshots = new List<TransformSnapshot>();

        private TransformSnapshot _current;

        public List<TransformSnapshot> Snapshots => _snapshots;
        float totalRecordedTime = 5f; // e.g. keep last 5 seconds

        void FixedUpdate() {
            
            if (IsReversing) return;
            
            float timeNow = Time.time;

            _snapshots.Add(new TransformSnapshot {
                time = timeNow,
                position = transform.position,
                rotation = transform.rotation
            });

            // Trim old entries
            while (_snapshots.Count > 0 && timeNow - _snapshots[0].time > totalRecordedTime)
                _snapshots.RemoveAt(0);
        }


        public void StartRecording()
        {
          
        }

        public void StopRecording()
        {
          
        }

        public void Clear()
        {
            _snapshots.Clear();
        }

        public List<TransformSnapshot> GetSnapshots()
        {
            return _snapshots;
        }
        
         


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            foreach (TransformSnapshot snapshot in _snapshots)
            {
                Gizmos.DrawSphere(snapshot.position, 0.1f);
            }
        }
    }
}