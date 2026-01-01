using UnityEngine;

public class CameraTargetSmooth : MonoBehaviour
{
    public Transform Target => _target;
    
    [SerializeField] private Transform _initialTarget; // on the player
    [SerializeField] private Transform _target;

    [SerializeField] private bool _isEnabled = false;
    
    
    
    [Tooltip("Higher = snappier. 12-25 is a good range.")]
    [SerializeField] private float _positionSharpness = 18f;

    [Tooltip("Higher = snappier. 12-30 is a good range.")]
    [SerializeField] private float _rotationSharpness = 22f;

     
     private bool _limitSpeed => _isEnabled;
    [SerializeField] private float _maxSpeed = 25f;            // meters/sec
    [SerializeField] private bool _limitAngularSpeed = true;
    [SerializeField] private float _maxAngularSpeedDeg = 360f; // deg/sec

    
    void Start()
    {
        _target.position = _initialTarget.position;
        _target.rotation = _initialTarget.rotation;
    }

    private void LateUpdate()
    {
      //  if (!_isEnabled) return;

        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // --- Position (exp smoothing) ---
        Vector3 currentPos = _target.position;
        Vector3 desiredPos = _initialTarget.position;

        float posAlpha = 1f - Mathf.Exp(-_positionSharpness * dt);
        Vector3 newPos = Vector3.Lerp(currentPos, desiredPos, posAlpha);

        if (_limitSpeed)
        {
            Vector3 delta = newPos - currentPos;
            float maxDelta = _maxSpeed * dt;
            float mag = delta.magnitude;
            if (mag > maxDelta && mag > 0.000001f)
                newPos = currentPos + delta * (maxDelta / mag);
        }

        // --- Rotation (exp smoothing) ---
        Quaternion currentRot = _target.rotation;
        Quaternion desiredRot = _initialTarget.rotation;

        float rotAlpha = 1f - Mathf.Exp(-_rotationSharpness * dt);
        Quaternion newRot = Quaternion.Slerp(currentRot, desiredRot, rotAlpha);

        if (_limitAngularSpeed)
        {
            float angle = Quaternion.Angle(currentRot, newRot);
            float maxAngle = _maxAngularSpeedDeg * dt;
            if (angle > maxAngle && angle > 0.0001f)
            {
                float t = maxAngle / angle;
                newRot = Quaternion.Slerp(currentRot, newRot, t);
            }
        }

        _target.SetPositionAndRotation(newPos, newRot);
    }

    public void OnRewind(bool on)
    {
        _isEnabled = on;
    }
}
