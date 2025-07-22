using System;
using UnityEngine;

namespace StarterAssets.ScriptableObjects
{
    [CreateAssetMenu(fileName = "RewindChannelSO", menuName = "SO/RewindChannel")]
    public class RewindEventChannelSO: ScriptableObject
    {
        public event Action OnRewindStart;
        public event Action OnRewindEnd;
        public event Action<float> OnRewindTick;

        public void RaiseRewindStarted() => OnRewindStart?.Invoke();
        public void RaiseRewindEnded() => OnRewindEnd?.Invoke();
        public void RaiseRewindTick(float time) => OnRewindTick?.Invoke(time);
        
        
    }
}