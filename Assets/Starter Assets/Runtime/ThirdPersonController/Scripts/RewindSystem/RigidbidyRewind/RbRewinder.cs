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
        
        private RigidbodyRecorder _recorder;
        
        private void OnValidate()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Awake()
        {
            _channel.OnRewindStart += OnRewindStart;
            _channel.OnRewindEnd += OnRewindEnd;
            _channel.OnRewindTick += OnRewindTick;
            
            _recorder = new RigidbodyRecorder(_rb);
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
            _rb.velocity = snapshot.Velocity;
            _rb.angularVelocity = snapshot.AngularVelocity;
        }
    }
}