using UnityEngine;

namespace StarterAssets.ScriptableObjects
{
    [CreateAssetMenu(fileName = "RewindSettingsSO", menuName = "SO/RewindSettings")]
    public class RewindSettingsSO: ScriptableObject
    {
        [SerializeField] private float _rewindSpeed = 1f;
        [SerializeField] private float _maxTimeRecord = 10f;

        public float RewindSpeed => _rewindSpeed;
        public float MaxTimeRecord => _maxTimeRecord;

        public static float Test = 1;
    }
}