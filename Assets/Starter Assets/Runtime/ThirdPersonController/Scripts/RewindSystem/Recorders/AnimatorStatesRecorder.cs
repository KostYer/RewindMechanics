using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using StarterAssets.Interfaces;
using UnityEditor.Animations;
using UnityEngine;

namespace Starter_Assets.Runtime.ThirdPersonController.Scripts.RewindSystem.Recorders
{
 
    public class AnimatorStateSnapshot
    {
        public float Time;
        public int StateHash;
        public float NormTime;
        public Dictionary<int, object> Values;
    }

    public class AnimatorStatesRecorder: IRecorder<AnimatorStateSnapshot>
    {
       public float MaxDuration { get; private set; }

       private List<AnimatorStateSnapshot> _frames = new();
       private CancellationTokenSource _tokenSource;
       private Animator _animator;

       private int _animIDSpeed = Animator.StringToHash("Speed");
       private int _animIDGrounded = Animator.StringToHash("Grounded");
       private int _animIDJump = Animator.StringToHash("Jump");
       private int _animIDFreeFall = Animator.StringToHash("FreeFall");
       private int _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
       
       private Dictionary<int, Func<object>> _animatorValueGetters = new();
       private Dictionary<int, Action<object>> _animatorValueSetters = new();
       
       public  Dictionary<int, Action<object>> AnimatorValueSetters => _animatorValueSetters;

       private Dictionary<int, string> _stateHashToName = new();
       
       private void BuildStateHashDictionary()
       {
           var controller = _animator.runtimeAnimatorController as AnimatorController;
           if (controller == null) return;

           foreach (var layer in controller.layers)
           {
               foreach (var state in layer.stateMachine.states)
               {
                   int hash = Animator.StringToHash(state.state.name);
                   _stateHashToName[hash] = state.state.name;
               }
           }
       }

       private void ConstructDictionaries()
       {
           _animatorValueGetters[_animIDSpeed] = () => _animator.GetFloat(_animIDSpeed);
           _animatorValueGetters[_animIDGrounded] = () => _animator.GetBool(_animIDGrounded);
           _animatorValueGetters[_animIDJump] = () => _animator.GetBool(_animIDJump);
           _animatorValueGetters[_animIDFreeFall] = () => _animator.GetBool(_animIDFreeFall);
           _animatorValueGetters[_animIDMotionSpeed] = () => _animator.GetFloat(_animIDMotionSpeed);
           
           _animatorValueSetters[_animIDSpeed] = (value) => _animator.SetFloat(_animIDSpeed, (float)value);
           _animatorValueSetters[_animIDGrounded] = (value) => _animator.SetBool(_animIDGrounded, (bool)value);
           _animatorValueSetters[_animIDJump] = (value) => _animator.SetBool(_animIDJump, (bool)value);
           _animatorValueSetters[_animIDFreeFall] = (value) => _animator.SetBool(_animIDFreeFall, (bool)value);
           _animatorValueSetters[_animIDMotionSpeed] = (value) => _animator.SetFloat(_animIDMotionSpeed, (float)value);
       }


       public AnimatorStatesRecorder(Animator animator, float maxRecTime)
        {
            _animator = animator;
            MaxDuration = maxRecTime;

            ConstructDictionaries();
            BuildStateHashDictionary();
        }

        public void StartRecording()
        {
         
            Clear();
            _tokenSource?.Cancel();  
            _tokenSource?.Dispose();
            _tokenSource = new CancellationTokenSource();
            
            RecordSnapshots(_tokenSource.Token).Forget();
        }

        public void StopRecording()
        { 
            _tokenSource?.Cancel();  
            _tokenSource?.Dispose();
            _tokenSource = null;
        }

        public void Clear()
        {
           
            _frames.Clear();
        }

        public List<AnimatorStateSnapshot> GetSnapshots()
        {
            return _frames;
        }
        
        private async UniTaskVoid RecordSnapshots(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                float timeNow = Time.time;
                RecordFrame(timeNow);

                while (_frames.Count > 0 && timeNow - _frames[0].Time > MaxDuration)
                      _frames.RemoveAt(0);

                await UniTask.Yield(PlayerLoopTiming.PreLateUpdate, token);
            }
        }

        private void RecordFrame(float time)
        {
            var dict = new Dictionary<int, object>();
            foreach (var kvp in _animatorValueGetters)
            {
                dict[kvp.Key] = kvp.Value.Invoke();
            }
            var snapshot = new AnimatorStateSnapshot();
            snapshot.Time = time;
            snapshot.Values = dict;
            
            snapshot.StateHash = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            snapshot.NormTime = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
           
            _frames.Add(snapshot);
        }

        public AnimatorStateSnapshot GetSnapshotAt(float time)
        {
            if (_frames == null || _frames.Count == 0)
            {
                Debug.LogError("[AnimatorStatesRecorder] GetSnapshotAt has no frames recorded");
                return null;
            }
        
            AnimatorStateSnapshot closest = _frames[0];
            float smallestDiff = Mathf.Abs(_frames[0].Time - time);
         
            for (int i = 1; i < _frames.Count; i++)
            {
                float diff = Mathf.Abs(_frames[i].Time - time);

                if (diff < smallestDiff)
                {
                    smallestDiff = diff;
                    closest = _frames[i];
                }
            }

            return closest;
        }
    }
}