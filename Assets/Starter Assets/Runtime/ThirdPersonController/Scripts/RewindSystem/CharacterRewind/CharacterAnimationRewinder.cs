using System.Collections.Generic;
using Recorders;
using RewindSystem.RuntimeAnimation;
using Starter_Assets.Runtime.ThirdPersonController.Scripts.RewindSystem.Recorders;
using StarterAssets.Interfaces;
using StarterAssets.ScriptableObjects;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using FrameData = Recorders.FrameData;

namespace RewindSystem
{
    public class CharacterAnimationRewinder: MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private Animator ghostAnimator;
        [SerializeField] private RewindSettingsSO _rewindSettings;
 
        private bool _isRewinding;
        private BonesProvider _bonesProvider;
        private ClipCreator _clipCreator = new ClipCreator();
      
        private PlayableGraph _graph;
        private AnimationClipPlayable _playableClip;
        private AnimationPlayableOutput _output;

        private IRecorder<FrameData> _animationRecorder;
        private IRecorder<AnimatorStateSnapshot> _animStatesRecorder;

        private float _targetTime;

        private Dictionary<int, string> _stateHashToName = new();
        
        private void BuildStateHashDictionary()
        {
#if UNITY_EDITOR
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
#endif
        }

        private void Awake()
        {
            BuildStateHashDictionary();
        }


        private void OnValidate()
        {
            _animator = GetComponentInParent<Animator>();
        }

        void Start()
        {
            _bonesProvider = new BonesProvider(_animator);
            _animationRecorder = new CharacterAnimationRecorder (_bonesProvider, _rewindSettings.MaxTimeRecord, _animator);
            _animStatesRecorder = new AnimatorStatesRecorder(_animator, _rewindSettings.MaxTimeRecord);
           
            _animationRecorder.StartRecording();
            _animStatesRecorder.StartRecording();
            
            _graph = PlayableGraph.Create("RewindGraph");
            _output = AnimationPlayableOutput.Create(_graph, "Animation", ghostAnimator);
        }
   
        public void OnRewindStart()
        {
            _animationRecorder.StopRecording();
            _animStatesRecorder.StopRecording();
           // _animator.enabled = false;
     
            _isRewinding = true;
            PlayAnimationClip();
        }
        
        public void OnRewindStop()
        {
            _animationRecorder.StartRecording();
            _animStatesRecorder.StartRecording();
            _isRewinding = false;
          
        }
     
        private void PlayAnimationClip()
        {
            var clip = _clipCreator.CreateAnimationClipFromFrames(_animator, _animationRecorder.GetSnapshots(), _bonesProvider.BoneMap);
            _playableClip = AnimationClipPlayable.Create(_graph, clip);
            _playableClip.SetDuration(clip.length);
            _playableClip.SetTime(clip.length);;
            _playableClip.SetSpeed(_rewindSettings.RewindSpeed * -1f); 

            _output.SetSourcePlayable(_playableClip);
            _graph.Play();
        }
  
        public void ApplyAnimationState(float targetTime)
        {
            var recorder = (AnimatorStatesRecorder)_animStatesRecorder;
            var frame = recorder.GetSnapshotAt(targetTime);
            
            foreach (var kvp in frame.Values)
            {
                if (recorder.AnimatorValueSetters.TryGetValue(kvp.Key, out var setter))
                    setter.Invoke(kvp.Value);
            }
 
            var stateHash = frame.StateHash;
            
            _animator.Play(stateHash, 0, frame.NormTime);
            _animator.Update(0f);
            _animator.enabled = true;
        }
    }
}