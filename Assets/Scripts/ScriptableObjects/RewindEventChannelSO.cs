using System;
using DebugScripts;
using UnityEngine;

namespace StarterAssets.ScriptableObjects
{
    [CreateAssetMenu(fileName = "RewindChannelSO", menuName = "SO/RewindChannel")]
    public class RewindEventChannelSO: ScriptableObject
    {
        public event Action OnRewindStart;
        public event Action OnRewindEnd;
        public event Action<float> OnRewindTick;
        public event Action<Vector3> OnImpact;
        public event Action<ProjectileData> OnBulletEnd;
        public event Action<GizmosRequest> OnGizmosRequest;
        public event Action  OnRbDestroyed = default;
        

        public void RaiseRewindStarted() => OnRewindStart?.Invoke();
        public void RaiseRewindEnded() => OnRewindEnd?.Invoke();
        public void RaiseRewindTick(float time) => OnRewindTick?.Invoke(time);
        public void OnImpactHit(Vector3 point) => OnImpact?.Invoke(point);
        public void OnBulletEnded(ProjectileData data) => OnBulletEnd?.Invoke(data);
        public void OnGizmosRequested(Vector3 pos, Color color, float size = .3f) =>OnGizmosRequest?.Invoke(new GizmosRequest(pos, color, size));
        
       
        
    }
}