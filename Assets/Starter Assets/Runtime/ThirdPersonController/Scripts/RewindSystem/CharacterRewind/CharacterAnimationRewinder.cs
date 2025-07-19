using System.Linq;
using Recorders;
using RewindSystem.RuntimeAnimation;
using StarterAssets.Interfaces;
using StarterAssets.ScriptableObjects;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using FrameData = Recorders.FrameData;

namespace RewindSystem
{
    public class CharacterAnimationRewinder: MonoBehaviour
    {
        [SerializeField] private Animator ghostAnimator;
        [SerializeField] private RewindSettingsSO _rewindSettings;
        [SerializeField] private Animator _animator;
 
        private bool _isRewinding;
        private BonesProvider _bonesProvider;
        private ClipCreator _clipCreator = new ClipCreator();
      
        private PlayableGraph _graph;
        private AnimationClipPlayable _playableClip;
        private AnimationPlayableOutput _output;

        private IRecorder<FrameData> _animationRecorder;
        
        private void OnValidate()
        {
            _animator = GetComponentInParent<Animator>();
        }

        void Start()
        {
            _bonesProvider = new BonesProvider(_animator);
            _animationRecorder = new CharacterAnimationRecorder (_bonesProvider, _rewindSettings.MaxTimeRecord);
           
            
            _animationRecorder.StartRecording();
            _graph = PlayableGraph.Create("RewindGraph");
            _output = AnimationPlayableOutput.Create(_graph, "Animation", ghostAnimator);
        }
   
        public void OnRewindStart()
        { _animationRecorder.StopRecording();
            
     
            _isRewinding = true;
            PlayAnimationClip();
        }
        
        public void OnRewindStop()
        {
            _animationRecorder.StartRecording();
            _isRewinding = false;
        }
     
        private void PlayAnimationClip()
        {
            var clip = _clipCreator.CreateAnimationClipFromFrames(_animator, _animationRecorder.GetSnapshots(), _bonesProvider.BoneMap);
             Debug.Log($"[PlayAnimationClip] clip.length {clip.length}  ");
            _playableClip = AnimationClipPlayable.Create(_graph, clip);
            _playableClip.SetDuration(clip.length);
            _playableClip.SetTime(clip.length);;
            _playableClip.SetSpeed(_rewindSettings.RewindSpeed * -1f); 

            _output.SetSourcePlayable(_playableClip);
            _graph.Play();
        }
    }
}