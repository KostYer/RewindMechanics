using System.Collections.Generic;
using Abilities;
using StarterAssets.ScriptableObjects;
using UnityEngine;

namespace RewindSystem.Recorders
{
    public class ProjectileRecorder: MonoBehaviour
    {
        [SerializeField] private RewindEventChannelSO _eventChannel;
        [SerializeField] private RewindInvoker _rewindInvoker;
        [SerializeField] private Projectile _prefab;

        private Stack<ProjectileData> _snapshots = new Stack<ProjectileData>();

        private float _epsilon = .1f;

        void Start()
        {
            _eventChannel.OnRewindTick += OnRewindTick;
            _eventChannel.OnBulletEnd += OnBulletEnd;
        }

        private void OnBulletEnd(ProjectileData data)
        {
            Debug.Log($"[ProjectileRecorder] OnBulletEnd");
            _snapshots.Push(data);
        }

        private void OnRewindTick(float t)
        {
            if (_snapshots.Count == 0)
                return;

            ProjectileData data = _snapshots.Peek();
            if (Mathf.Abs(t - data.EndTime) <= _epsilon)
            {
                _snapshots.Pop();
              //  _eventChannel.OnGizmosRequested(data.EndPosition, Color.cyan, .15f); 
                var go = Instantiate(_prefab, data.EndPosition, Quaternion.identity);
                go.OnReverse(data);
            }
        }
    }
}

public struct ProjectileData
{
    public Vector3 StartPosition;
    public Vector3 EndPosition;
    public float EndTime;
    public float FromTime;
    public float ForceLen;
}