using UnityEngine;

namespace StarterAssets.ScriptableObjects
{
    [CreateAssetMenu(fileName = "RewindSettingsSO", menuName = "SO/RewindSettings")]
    public class RewindSettingsSO: ScriptableObject
    {
        [SerializeField] private float _rewindSpeed = 1;

        public float RewindSpeed => _rewindSpeed;
    }
}