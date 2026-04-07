using Abilities;
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
        [SerializeField] private Projectile _projectile;
        
        private RigidbodyRecorder _recorder;

        private Vector3 _currentFrameVelocity;
        private Vector3 _currentFrameAngVelocity;
        
        public bool HasSnapshots => _recorder.HasSnapshots;
        
        public float FirstSnapshotTime => _recorder.FirstSnapshotTime;
        public float LastSnapshotTime  => _recorder.LastSnapshotTime;

        
        private void OnValidate()
        {
            _rb = GetComponent<Rigidbody>();
            if (TryGetComponent<Projectile>(out var p))
            {
                _projectile = p;
            }
        }

        private void Awake()
        {
            _channel.OnRewindStart += OnRewindStart;
            _channel.OnRewindEnd += OnRewindEnd;
            _channel.OnRewindTick += OnRewindTick;
            
            _recorder = new RigidbodyRecorder(_rb, _rewindSettings.MaxTimeRecord);
        }

        private void OnDestroy()
        {
            _channel.OnRewindStart -= OnRewindStart;
            _channel.OnRewindEnd -= OnRewindEnd;
            _channel.OnRewindTick -= OnRewindTick;
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
            
            if (_recorder.HasSnapshots &&
                (time < _recorder.FirstSnapshotTime || time > _recorder.LastSnapshotTime))
            {
                Debug.LogWarning(
                    $"[RbRewinder:{name}] time out of range: {time:F3} " +
                    $"(range {_recorder.FirstSnapshotTime:F3}–{_recorder.LastSnapshotTime:F3})");
                return;
            }
            
            
             var snapshot = _recorder.FindClosestSnapshot(time);
             ApplySnapshot(snapshot);
        }
        
        public void ApplySnapshot(RbSnapshot snapshot)
        {
            if (_projectile)
            {
                if(!_projectile.IsEnabled)  _projectile.EnableProjectile();
                    
            }

            _rb.position = snapshot.Position;
            _rb.rotation = snapshot.Rotation;
            _currentFrameVelocity= snapshot.Velocity;
            _currentFrameAngVelocity = snapshot.AngularVelocity;
        }
    }
}