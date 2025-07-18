using System.Collections.Generic;
using Recorders;
using StarterAssets.ScriptableObjects;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace RewindSystem
{
    public class CharacterAnimationRewinder: MonoBehaviour
    {
        [SerializeField] private CharacterAnimationRecorder characterAnimationRecorder;
        [SerializeField] private Animator ghostAnimator;
        [SerializeField] private RewindSettingsSO _rewindSettings;
 
        private bool _isRewinding;
      
        private PlayableGraph _graph;
        private AnimationClipPlayable _playableClip;
        private AnimationPlayableOutput _output;
        
        void Start()
        {
            _graph = PlayableGraph.Create("RewindGraph");
            _output = AnimationPlayableOutput.Create(_graph, "Animation", ghostAnimator);
        }
   
        public void OnRewindStart()
        {
            characterAnimationRecorder.OnRewindStart();
     
            _isRewinding = true;
            PlayAnimationClip();
        }
        
        public void OnRewindStop()
        {
            characterAnimationRecorder.OnRewindStop();
            _isRewinding = false;
        }
     
        private void PlayAnimationClip()
        {
            var clip = characterAnimationRecorder.CreateAnimationClipFromFrames();
       
            _playableClip = AnimationClipPlayable.Create(_graph, clip);
            _playableClip.SetDuration(clip.length);
            _playableClip.SetTime(clip.length);;
            _playableClip.SetSpeed(_rewindSettings.RewindSpeed * -1f); 

            _output.SetSourcePlayable(_playableClip);
            _graph.Play();
        }
    }
}