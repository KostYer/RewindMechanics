using System;
using UnityEngine;

namespace Abilities
{
    public class Projectile: MonoBehaviour
    {
        [SerializeField] private Transform _lookTarget;
        [SerializeField] private GameObject _impactQuad; 
        private float _explodeForce;
        private float _explosionRadius = 2f;
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private LayerMask _affectsLayers;

        private void OnValidate()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void OnShoot(Vector3 moveForce, float explodeForce)
        {
            _explodeForce = explodeForce;
            _rb.AddForce(moveForce, ForceMode.Impulse);
        }


        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[Projectile] OnTriggerEnter");
            
            Explode();
            
            Vector3 pos = other.ClosestPoint(transform.position);
            ShowImpact(pos);

        }

        public void SetTarget(Transform target)
        {
            _lookTarget = target;
        }

      

        private void Explode()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, _explosionRadius, _affectsLayers);

            foreach (var hit in hits)
            {
                Rigidbody rb = hit.attachedRigidbody;
                if (rb == null) continue;

                // Apply explosion force
                rb.AddExplosionForce(
                    _explodeForce,
                    transform.position,
                    _explosionRadius,
                    0.5f, // optional upward modifier
                    ForceMode.Impulse
                );
            }
          
            Destroy(gameObject);
        }

        private void ShowImpact(Vector3 pos)
        {
            var go = Instantiate(_impactQuad, pos, Quaternion.identity);
            
           
        }
    }
}