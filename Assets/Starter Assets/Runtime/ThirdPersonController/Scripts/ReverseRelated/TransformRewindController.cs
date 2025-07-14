using System;
using UnityEngine;

namespace ReverseRelated
{
    public class TransformRewindController: MonoBehaviour
    {
       [SerializeField] private Reversible _reversible; //rename to transform recorder
       [SerializeField] private CharacterController _controller;
        private Rewinder _rewinder;

        private bool _isRewinding;
        
        public void OnRewindStart()
        {
            Debug.Log($"[TransformRewindController] OnRewindStart");
            _controller.enabled = false;
            _isRewinding = true;
            _reversible.IsReversing = true;
          
        }

        public void OnRewindStop()
        {
            _controller.enabled = true;
            _isRewinding = false;
            _reversible.IsReversing = false;
            _reversible.Snapshots.Clear();
        }

        private void Update()
        {
            if (!_isRewinding) return;

            ApplyRewind(_rewinder.GetRewindTargetTime());
        }

        public void Init(Rewinder rewinder)
        {
            _rewinder = rewinder;
        }

        private void ApplyRewind(float currentRewindTime)
        {
            for (int i = _reversible.Snapshots.Count - 1; i > 0; i--)
            {
                if (_reversible.Snapshots[i - 1].time <= currentRewindTime &&
                    currentRewindTime <= _reversible.Snapshots[i].time)
                {
                    var a = _reversible.Snapshots[i];
                    var b = _reversible.Snapshots[i - 1];

                    float t = Mathf.InverseLerp(a.time, b.time, currentRewindTime);
                    transform.position = Vector3.Lerp(a.position, b.position, t);
                    transform.rotation = Quaternion.Slerp(a.rotation, b.rotation, t);
                    break;
                }
            }
        } 

       
    }
}