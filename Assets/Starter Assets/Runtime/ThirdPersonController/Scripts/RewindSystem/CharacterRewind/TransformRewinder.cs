using UnityEngine;
 

namespace ReverseRelated
{
    public class TransformRewinder: MonoBehaviour
    {
       [SerializeField] private TransformRecorder transformRecorder; //rename to transform recorder
       [SerializeField] private CharacterController _controller;

        private bool _isRewinding;
        
        public void OnRewindStart()
        {
            _controller.enabled = false;
            _isRewinding = true;
            transformRecorder.IsReversing = true;
          
        }

        public void OnRewindStop()
        {
            _controller.enabled = true;
            _isRewinding = false;
            transformRecorder.IsReversing = false;
            transformRecorder.Snapshots.Clear();
        }
 
 
        public void ApplyRewind(float currentRewindTime)
        {
            for (int i = transformRecorder.Snapshots.Count - 1; i > 0; i--)
            {
                if (transformRecorder.Snapshots[i - 1].time <= currentRewindTime &&
                    currentRewindTime <= transformRecorder.Snapshots[i].time)
                {
                    var a = transformRecorder.Snapshots[i];
                    var b = transformRecorder.Snapshots[i - 1];

                    float t = Mathf.InverseLerp(a.time, b.time, currentRewindTime);
                    _controller.transform.position = Vector3.Lerp(a.position, b.position, t);
                    _controller.transform.rotation = Quaternion.Slerp(a.rotation, b.rotation, t);
                    break;
                }
            }
        } 
    }
}