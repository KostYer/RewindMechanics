using Other;
using StarterAssets.ScriptableObjects;
using UnityEngine;

namespace Abilities
{
    public class Projectile: MonoBehaviour
    {
        public Vector3 InitialForce => _initForce;
        public float ExplodeForce => _explodeForce;

        public Rigidbody Rigidbody => _rb;
        
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private LayerMask _affectsLayers;
        [SerializeField] private Transform _lookTarget;
        [SerializeField] private GameObject _impactQuad;
        [SerializeField] private RewindEventChannelSO _eventChannel;
        [SerializeField] private Collider _collider;
        [SerializeField] private MeshRenderer _renderer;
        
        
        
        private float _explodeForce;
        private float _explosionRadius = 2f;

        private float _timeStart;
        private float _timeEnd;
        private Vector3 _startPos;
        private Vector3 _initForce;


        #region timeline

        public int TrackId { get; private set; } = -1;
        private ProjectileTimelineManager _timelineManager;

        public void BindTrack(int trackId, ProjectileTimelineManager timelineManager)
        {
            TrackId = trackId;
            _timelineManager = timelineManager;
        }
        
        public void NotifyEnded(float currentTime)
        {
            if (_timelineManager == null)
            {
                Debug.LogError($"{name}: ProjectileTimelineManager is not assigned.");
                return;
            }

            if (TrackId < 0)
            {
                Debug.LogError($"{name}: Projectile TrackId is invalid.");
                return;
            }

            _timelineManager.MarkProjectileDestroyed(TrackId, currentTime);
        }

        #endregion timeline
       
        

        private void OnValidate()
        {
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _renderer = GetComponent<MeshRenderer>();
        }


        public void OnShoot(Vector3 moveForce, float explodeForce)
        {
            _initForce = moveForce;
            _startPos = transform.position;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            
            _explodeForce = explodeForce;
            _rb.AddForce(moveForce, ForceMode.Impulse);

            _timeStart = Time.time;
        }


        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[Projectile] OnTriggerEnter");
            
            Explode();

            Vector3 pos = transform.position;// other.ClosestPoint(transform.position);
            ShowImpact(pos);
            
            _eventChannel.OnImpactHit(pos);

        }

        public void SetTarget(Transform target)
        {
            _lookTarget = target;
        }

        private void SendBulletSnapshot()
        {
            Debug.Log($"[Projectile] SendBulletSnapshot position {transform.position}");
          
            _timeEnd = Time.time;

            _data = new ProjectileData
            {
                StartPosition = _startPos,
                EndPosition = transform.position,
                EndTime = Time.time,
                FromTime = _timeStart,
                ForceLen = _rb.velocity.magnitude
            };
            
          //  _eventChannel.OnGizmosRequested(_data.EndPosition, Color.blue, .2f); 
            _eventChannel.OnBulletEnded(_data);
        }

        private void Explode()
        {
           
            SendBulletSnapshot();
            
            Collider[] hits = Physics.OverlapSphere(transform.position, _explosionRadius, _affectsLayers);

            foreach (var hit in hits)
            {
                Rigidbody rb = hit.attachedRigidbody;
                if (rb == null) continue;

                // Apply explosion force
                rb.AddExplosionForce(_explodeForce, transform.position, _explosionRadius,0.5f, ForceMode.Impulse);
            }

            DisableProjectile();
            //  Destroy(gameObject);
        }

        public bool IsEnabled => _isEnabled;
        
        private bool _isEnabled = true;
        private Vector3 _cacheVelocity;
        private Vector3 _cacheVelocityAngular;
        
        
        private void DisableProjectile()
        {
            _isEnabled = false;
          
            _cacheVelocity = _rb.velocity;
            _cacheVelocityAngular = _rb.angularVelocity;
            
            _rb.velocity = Vector3.zero;
            _rb.useGravity = false;
            _renderer.enabled = false;

            NotifyEnded(Time.time);
        }

        public void EnableProjectile()
        {
            _isEnabled = true;
            _renderer.enabled = true;

            _rb.velocity = _cacheVelocity;
            _rb.angularVelocity = _cacheVelocityAngular;
        }

        private void ShowImpact(Vector3 pos)
        {
            var go = Instantiate(_impactQuad, pos, Quaternion.identity);

            go.GetComponent<ExplosionQuad>().OnCollided();
        }

        public void OnReverse(ProjectileData data)
        {
            return;
            _data = data;
            _eventChannel.OnGizmosRequested(transform.position, Color.cyan, .15f); 
            _isReversing = true;
        
            var direction = data.StartPosition - data.EndPosition;
            direction.Normalize();
            _rb.velocity = Vector3.zero;
            
           _rb.velocity = direction * (data.ForceLen);
            
            _collider.isTrigger = false;
        }

        private bool _isReversing;
        private ProjectileData _data;
        private const float _proximity = .3f;
        
        private void Update()
        {
            return;
             if(!_isReversing) return;
             _eventChannel.OnGizmosRequested(_data.StartPosition, Color.yellow, .15f); 
             if (Vector3.Distance(transform.position,_data.StartPosition )<= _proximity)
             {
                 _isReversing = false;
                 _rb.isKinematic = true;
                 gameObject.SetActive(false);
             }
        }
    }
}