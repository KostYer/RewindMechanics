using Abilities;
using StarterAssets.ScriptableObjects;
using UnityEngine;

namespace RewindSystem
{
    public class BulletRestarter: MonoBehaviour
    {
        [SerializeField] private RewindEventChannelSO _eventChannel;
        [SerializeField] private Projectile _projectile;
 
        private void Awake()
        {
            _eventChannel.OnRewindEnd += OnRewindEnd;
        }
 

        private void OnDestroy()
        {
            _eventChannel.OnRewindEnd  -= OnRewindEnd;
        }

        private void OnRewindEnd()
        {
            _projectile.OnShoot(_projectile.InitialForce, _projectile.ExplodeForce);
        }
    }
}