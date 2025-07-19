using Recorders;
using Snapshots;
using StarterAssets.ScriptableObjects;
using UnityEngine;

namespace RewindSystem.RigidbidyRewind
{
    public class RbRewinder: MonoBehaviour
    {
        [SerializeField] private RewindEventChannelSO _channel;
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private RewindSettingsSO _rewindSettings;
        
        private RigidbodyRecorder _recorder;

        private Vector3 _currentFrameVelocity;
        private Vector3 _currentFrameAngVelocity;
        
        private void OnValidate()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Awake()
        {
            _channel.OnRewindStart += OnRewindStart;
            _channel.OnRewindEnd += OnRewindEnd;
            _channel.OnRewindTick += OnRewindTick;
            
            _recorder = new RigidbodyRecorder(_rb, _rewindSettings.MaxTimeRecord);
        }

        private void Start()
        {
            _recorder.StartRecording();
        }

        private void OnRewindStart()
        {
            _recorder.StopRecording();
        }
        
        private void OnRewindEnd()
        {
            _recorder.StartRecording();
        //    _rb.velocity = _currentFrameVelocity;
       //     _rb.angularVelocity = _currentFrameAngVelocity;
        }
        
        private void OnRewindTick(float time)
        {
             var snapshot = _recorder.FindClosestSnapshot(time);
             ApplySnapshot(snapshot);
        }
        
        public void ApplySnapshot(RbSnapshot snapshot)
        {
            _rb.position = snapshot.Position;
            _rb.rotation = snapshot.Rotation;
            _currentFrameVelocity= snapshot.Velocity;
            _currentFrameAngVelocity = snapshot.AngularVelocity;
        }
    }
}