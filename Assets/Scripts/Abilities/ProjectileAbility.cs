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
        [SerializeField] private ProjectileTimelineManager _timelineManager;
        
        private void Shoot()
        {
           // _eventChannel.OnGizmosRequested( _shootPoint.position, Color.cyan, .5f); 
            var projectile = Instantiate(_projectile, _shootPoint.position, _shootPoint.rotation);

         
            float time = Time.time;
            var track = _timelineManager.RegisterProjectile(projectile, projectile.Rigidbody, time);
            projectile.BindTrack(track.Id, _timelineManager);
            
            var force = transform.forward * _speed;
            projectile.OnShoot(force, _explodeForce);
            
            projectile.SetTarget(transform);
        }
         


        public void OnFireAnimCallback()
        {
            Shoot();
         
        }



    }
}