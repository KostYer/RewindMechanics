using System.Collections.Generic;
using Other;
using RewindSystem;
using StarterAssets.ScriptableObjects;
using UnityEngine;

public class ImpactVfxRecorder : MonoBehaviour
{
    [SerializeField] private RewindEventChannelSO _eventChannel;
    [SerializeField] private RewindInvoker _rewindInvoker;
    [SerializeField] private ExplosionQuad _prefab;

    private Stack<Impact> _snapshots = new Stack<Impact>();

    private float _epsilon = .02f;

    private const float _lifeTimeCorrection = 0f;
    
    private void OnImpact(Vector3 pos)
    {
        _snapshots.Push(new Impact{Position = pos, Time = Time.time - _lifeTimeCorrection});
    }


    void Start()
    {
        _eventChannel.OnImpact += OnImpact;

        _eventChannel.OnRewindTick += OnRewindTick;
    }

    private void OnRewindTick(float t)
    {
        if (_snapshots.Count == 0)
            return;

        Impact impact = _snapshots.Peek();
        if (Mathf.Abs(t - impact.Time) <= _epsilon)
        {
            _snapshots.Pop();

           var go = Instantiate(_prefab, impact.Position, Quaternion.identity);
           go.OnReverse();

        }
    }
}


public struct Impact
{
    public Vector3 Position;
    public float Time;
}
