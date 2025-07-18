using System;
using Recorders;
using StarterAssets.ScriptableObjects;
using UnityEngine;

namespace RewindSystem.RigidbidyRewind
{
    public class RbRewinder: MonoBehaviour
    {
        [SerializeField] private RewindEventChannelSO _channel;
        [SerializeField] private RigidbodyRecorder _recorder;
       
        private void Awake()
        {
            _channel.OnRewindStart += OnRewindStart;
            _channel.OnRewindEnd += OnRewindEnd;
            _channel.OnRewindTick += OnRewindTick;
        }

        private void Start()
        {
            _recorder.StartRecord();
        }

        private void OnRewindStart()
        {
            _recorder.StopRecord();
        }
        
        private void OnRewindEnd()
        {
            _recorder.StartRecord();
        }
        
        private void OnRewindTick(float time)
        {
             var snapshot = _recorder.FindClosestSnapshot(time);
            _recorder.ApplyTo(snapshot);
        }


    }
}