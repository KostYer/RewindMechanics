using UnityEngine;

namespace Abilities
{
    public class Projectile: MonoBehaviour
    {
        private float _explodeForce;
        private float _explosionRadius = 10f;
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
            Explode();
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
    }
}