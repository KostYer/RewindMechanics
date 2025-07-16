using System;
using System.Collections.Generic;
using ReverseRelated.AnimRecording;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using FrameData = ReverseRelated.AnimRecording.FrameData;

namespace ReverseRelated
{
    public class RawAnimationRewindController: MonoBehaviour
    {
        [SerializeField] private RawFrameRecorder _rawFrameRecorder;
        
 
        
        private Rewinder _rewinder;
        
        
        private List<FrameData> recordedFrames = new List<FrameData>();

        private bool _isRewinding;
        
        
        
 
        private Dictionary<HumanBodyBones, Transform> ghostBones;
         
        [SerializeField] private float playbackSpeed = 1f;
        
        private int currentFrame;
        
        
        
        [SerializeField] private Animator ghostAnimator;
        private PlayableGraph graph;
        private AnimationClipPlayable playableClip;
        private AnimationPlayableOutput output;

        [SerializeField] private AnimationClip _debugClip;
        
        
        void Start()
        {
            /*ghostBones = new();
            foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
            {
                if (bone == HumanBodyBones.LastBone) continue;

                var t = ghostAnimator.GetBoneTransform(bone);
                if (t != null)
                    ghostBones[bone] = t;
            }*/
            
            
            graph = PlayableGraph.Create("RewindGraph");
            output = AnimationPlayableOutput.Create(graph, "Animation", ghostAnimator);
        }
     

        public void Init(Rewinder rewinder)
        {
            _rewinder = rewinder;
        }

        public void OnRewindStart()
        {
            _rawFrameRecorder.OnRewindStart();
         //   recordedFrames = _rawFrameRecorder.GetRecordedData();
         ///   currentFrame = recordedFrames.Count;
     
            _isRewinding = true;
            PlayAnimationClip();
        }

        [SerializeField] private AnimationClip _clipDebug2 = default;
        private void PlayAnimationClip()
        {
            Debug.Log($"[RawAnimationRewindController] PlayAnimationClip");

            var clip = _rawFrameRecorder.CreateAnimationClipFromFrames();
          //  recordedFrames.Clear();
          // Debug.Log($"[RawAnimationRewindController] clip is ok {clip != null}");

         // _clipDebug2 = clip;
          
          
    
            /*graph = PlayableGraph.Create("RewindGraph");
            var output = AnimationPlayableOutput.Create(graph, "Animation", ghostAnimator);*/

            playableClip = AnimationClipPlayable.Create(graph, clip);
            playableClip.SetDuration(clip.length);
           // playableClip.SetTime(0);
            playableClip.SetTime(clip.length);;
            playableClip.SetSpeed(-.5f); // negative = rewind

            output.SetSourcePlayable(playableClip);
            
            Debug.Log($"[RawAnimationRewindController] end");
            graph.Play();
        }

        public void OnRewindStop()
        {
            _rawFrameRecorder.OnRewindStop();
            _isRewinding = false;
        }

        private void LateUpdate()
        {
             /*if (!_isRewinding || recordedFrames == null || recordedFrames.Count == 0)
                return;  
            if (!_isRewinding) return;
            // Clamp and rewind
           currentFrame = Mathf.Max(currentFrame - Mathf.RoundToInt(Time.deltaTime * 60 * playbackSpeed), 0);

            ApplyFrame(recordedFrames[currentFrame]);

            if (currentFrame == 0)
                _isRewinding = false;*/
        }

        private void ApplyFrame(FrameData frame)
        {
            currentFrame--;
            foreach (var boneData in frame.bones)
            {
                if (ghostBones.TryGetValue(boneData.Key, out var t))
                {
                    t.localPosition = boneData.Value.localPosition;
                    t.localRotation = boneData.Value.localRotation;
                }
            }
        }
    }
}