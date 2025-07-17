using System.Collections.Generic;
using Snapshots;
using UnityEngine;

namespace RewindSystem
{
    public class TransformRecorder: MonoBehaviour
    {
        public bool IsReversing;
        private float _maxTime = 5f;
        List<TransformSnapshot> snapshots = new List<TransformSnapshot>();

        private TransformSnapshot _current;

        public List<TransformSnapshot> Snapshots => snapshots;
        float totalRecordedTime = 5f; // e.g. keep last 5 seconds

        void FixedUpdate() {
            
            if (IsReversing) return;
            
            float timeNow = Time.time;

            snapshots.Add(new TransformSnapshot {
                time = timeNow,
                position = transform.position,
                rotation = transform.rotation
            });

            // Trim old entries
            while (snapshots.Count > 0 && timeNow - snapshots[0].time > totalRecordedTime)
                snapshots.RemoveAt(0);
        }
 


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            foreach (TransformSnapshot snapshot in snapshots)
            {
                Gizmos.DrawSphere(snapshot.position, 0.1f);
            }
        }
    }
}