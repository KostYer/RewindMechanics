using System.Collections.Generic;
using StarterAssets.ScriptableObjects;
using UnityEngine;

namespace DebugScripts
{
    
    public class GizmosDrawer: MonoBehaviour
    {
        [SerializeField] private RewindEventChannelSO _eventChannel;
       
        private List<GizmosRequest> _positions = new List<GizmosRequest>();

       [SerializeField] private Color _color;

       private float _radius = .25f;

       private void Awake()
       {
           _eventChannel.OnGizmosRequest += OnGizmosRequest;
       }

       private void OnGizmosRequest(GizmosRequest request)
       {
           _positions.Add(request);
       }

       private void OnDrawGizmos()
        {
            if(_positions.Count == 0) return;

            
            foreach (var request in _positions)
            {
                Gizmos.color = request.Color;
                Gizmos.DrawSphere(request.Position, request.Size);
            }
        }
    }
    
    public class GizmosRequest
    {
        public Vector3 Position;
        public Color Color = Color.black;
        public float Size;

        public GizmosRequest(Vector3 pos, Color color, float size)
        {
            Position = pos;
            Color = color;
            Size = size;
        }
    }
 
}