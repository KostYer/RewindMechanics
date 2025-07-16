using System;
using UnityEngine;

namespace ReverseRelated
{
    public class Rewinder: MonoBehaviour
    {
     //   [SerializeField] private AnimationRewindController _animationRewindController;
        [SerializeField] private RawAnimationRewindController _rawAnimationRewindController;
        
        [SerializeField] private TransformRewindController _transformRewindController;
        [SerializeField] private SkinnedMeshRenderer  _skinnedMeshRenderer;
        [SerializeField] private SkinnedMeshRenderer  _skinnedMeshRendererGhost;


         
        [SerializeField] private float _debugTimeScale = 1f;

        private float rewindStartTime;
        
        private bool _isReversing;
        

        private float currentRewindTime;
        public float CurrentRewindTime => currentRewindTime;
        private float rewindElapsed;
        
        private void Awake()
        {
        //    _animationRewindController.Init(this);
            _rawAnimationRewindController.Init(this);
            _transformRewindController.Init(this);
            
            _skinnedMeshRendererGhost.enabled = false;
        }

        private void Update()
        {
             if(!_isReversing) return;
             
             
        }

        public void StartRewind()
        {
            Time.timeScale = _debugTimeScale;
          //  _controller.enabled = false;
            rewindStartTime = Time.time;                      // current Unity time
          ///  currentRewindTime = _reversible.Snapshots[_reversible.Snapshots.Count - 1].time;  // rewind from the most recent snapshot CHANGE
            _isReversing = true;
            
         
            rewindElapsed = 0f;
            _skinnedMeshRenderer.enabled = false;
            _skinnedMeshRendererGhost.enabled = true;



          ////  _reversible.IsReversing = true;
         //   animationRewindPlayer.OnRewindStart();
            
            
         //   _animationRewindController.OnRewindStart();
            _rawAnimationRewindController.OnRewindStart();
            _transformRewindController.OnRewindStart();
        }
        
        public void StopRewind()
        {
            Time.timeScale = 1;
            _isReversing = false;
            //   _controller.enabled = true;
            //   animationRewindPlayer.OnRewindStop();
            _skinnedMeshRenderer.enabled = true;
            _skinnedMeshRendererGhost.enabled = false;
         //   _animationRewindController.OnRewindStop();
            _rawAnimationRewindController.OnRewindStop();
            _transformRewindController.OnRewindStop();
        }

        
        private float rewindElapsedTime = 0f;

      

        

       
        public float GetRewindTargetTime()
        {
            rewindElapsed += Time.unscaledDeltaTime;
            float targetTime = rewindStartTime - rewindElapsed;

            if (targetTime <= 0f) // or another limit
            {
                Debug.Log($"[targetTime <= 0]");
                StopRewind();
                return 0f;
            }

            return targetTime;
        }
        
        
      
    }
}