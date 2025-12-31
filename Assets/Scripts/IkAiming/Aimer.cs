using UnityEngine;

namespace IkAiming
{
    public class Aimer: MonoBehaviour
    {
        public Vector3 Direction;
        [SerializeField] private Transform _aimRoot;
        [SerializeField] private LayerMask _targetLayer;
        [SerializeField] private Camera _camera;
        [SerializeField] private float _height = 1.5f;

        private bool _isAiming = true;
        private bool _isHit;
        private Vector3 _direction;

        private Transform _parent;

        private Quaternion _targetRot;
        private float _rotationSpeed = 360f;
        
        
        
        private Quaternion _desiredRotation;
        private bool _turnToShoot;
        private Vector3 _aimPoint;

        private void Awake()
        {
            _parent = transform.parent;
        }


        private void Update()
        {
             if(!_isAiming) return;

          
             
             if (Input.GetKeyDown(KeyCode.Mouse0))
             {
                 // 1. Get world aim point from camera center
                 Ray ray = _camera.ScreenPointToRay(
                     new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));

                 if (Physics.Raycast(ray, out RaycastHit hit, 1000f, _targetLayer))
                     _aimPoint = hit.point;
                 else
                     _aimPoint = ray.origin + ray.direction * 1000f;

                 _aimPoint.y = _parent.position.y + _height;
                 Debug.DrawLine(_aimRoot.position, _aimPoint, Color.red);

                 // 2. Desired body rotation: face the aim point (flat on Y)
                 Vector3 flatDir = _aimPoint - transform.position;
                 flatDir.y = 0f;

                 if (flatDir.sqrMagnitude > 0.0001f)
                 {
                     flatDir.Normalize();
                     _desiredRotation = Quaternion.LookRotation(flatDir);
                     _turnToShoot = true;
                 }

                 // 3. Trigger fire animation (blend will happen while he turns)
                 // _animator.SetTrigger("Fire");
             }

             // 4. Gradual turn to desired rotation
             if (_turnToShoot)
             {
                 _parent.transform.rotation = Quaternion.RotateTowards(
                     _parent.transform.rotation,
                     _desiredRotation,
                     _rotationSpeed * Time.deltaTime);

                 if (Quaternion.Angle(_parent.transform.rotation, _desiredRotation) < 0.5f)
                 {
                     _turnToShoot = false;
                 }
             }

        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_aimPoint, .15f);
        }
    }
}