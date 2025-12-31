using StarterAssets.ScriptableObjects;
using UnityEngine;

namespace RewindSystem
{
    public class TransformRewinder: MonoBehaviour
    {
       [SerializeField] private CharacterController _controller;
       [SerializeField] private RewindSettingsSO _rewindSettings;
       [SerializeField] private Transform _target;
       private TransformRecorder _transformRecorder;  

        private void Start()
        {
            _transformRecorder = new TransformRecorder(_controller.transform, _rewindSettings.MaxTimeRecord);
            _transformRecorder.StartRecording();
        }

        public void OnRewindStart()
        {
            _controller.enabled = false;
            _transformRecorder.StopRecording();
        }

        public void OnRewindStop()
        {
            _controller.enabled = true;
            _transformRecorder.StartRecording();
        }
 
        public void ApplyRewind(float currentRewindTime)
        {
            for (int i = _transformRecorder.Snapshots.Count - 1; i > 0; i--)
            {
                if (_transformRecorder.Snapshots[i - 1].time <= currentRewindTime &&
                    currentRewindTime <= _transformRecorder.Snapshots[i].time)
                {
                    var a = _transformRecorder.Snapshots[i];
                    var b = _transformRecorder.Snapshots[i - 1];

                    float t = Mathf.InverseLerp(a.time, b.time, currentRewindTime);
                    _target.position = Vector3.Lerp(a.position, b.position, t);
                    _target.rotation = Quaternion.Slerp(a.rotation, b.rotation, t);
                    break;
                }
            }
        } 
    }
}