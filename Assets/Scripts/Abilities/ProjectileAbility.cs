using StarterAssets.ScriptableObjects;
using UnityEngine;

namespace Abilities
{
    public class ProjectileAbility: MonoBehaviour
    {
        [SerializeField] private float _speed = 550f;
        [SerializeField] private float _explodeForce = 40f;

        [SerializeField] private Transform _shootPoint;
        [SerializeField] private Projectile _projectile;

        [SerializeField] private RewindEventChannelSO _eventChannel;
        
        private void Shoot()
        {
           // _eventChannel.OnGizmosRequested( _shootPoint.position, Color.cyan, .5f); 
            var go = Instantiate(_projectile, _shootPoint.position, _shootPoint.rotation);
            var force = transform.forward * _speed;
            go.OnShoot(force, _explodeForce);
            
            go.SetTarget(transform);
        }
         


        public void OnFireAnimCallback()
        {
            Shoot();
         
        }



    }
}