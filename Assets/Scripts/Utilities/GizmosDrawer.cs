using UnityEngine;

public class GizmosDrawer : MonoBehaviour
{
    [SerializeField] private bool _isEnabled = true;
    [SerializeField] private float _radius = .4f;
    [SerializeField] private Color _color;
    [SerializeField] private Color _lineColor;
    [SerializeField] private float _len;

    private void OnDrawGizmos()
    {
       if(!_isEnabled) return;
       
       Gizmos.color = _color;
       
       Gizmos.DrawSphere(transform.position, _radius);
       
       Gizmos.color = _lineColor;
       Gizmos.DrawLine(transform.position, transform.forward * _len);


    }
}
