using UnityEngine;

public class CameraFolower : MonoBehaviour
{
    [SerializeField] private bool _isEnabled;
    [SerializeField] private float _lifetime = .5f;
    private Camera _camera;

    private float _span; 
    
    private void Start()
    {
        _span = _lifetime;
        _camera = Camera.main;
      
    }

    void LateUpdate()
    {
        if(!_isEnabled) return;
        _span -= Time.deltaTime;
        if (_span <= 0f)
        {
            _isEnabled = false;
            gameObject.SetActive(false);
            return;
        }

        Vector3 direction = (_camera.transform.position - transform.position).normalized;
        transform.forward = -direction;
        
    }
    
    
}
