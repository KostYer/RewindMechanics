using StarterAssets.ScriptableObjects;
using UnityEngine;

namespace RewindSystem
{
    public class PlayerRewinder: MonoBehaviour
    {
        [SerializeField] private RewindEventChannelSO _rewindEventChannel;
        [SerializeField] private SkinnedMeshRenderer  _skinnedMeshRenderer;
        [SerializeField] private SkinnedMeshRenderer  _skinnedMeshRendererGhost;
        [SerializeField] private CharacterAnimationRewinder characterAnimationRewinder;
        [SerializeField] private TransformRewinder transformRewinder;

        private float rewindStartTime;
        private bool _isReversing;
        private float rewindElapsed;
        
        private float rewindSpeed = .5f;
        
        private void Awake()
        {
            _skinnedMeshRendererGhost.enabled = false;
            _rewindEventChannel.OnRewindStart += StartRewind;
            _rewindEventChannel.OnRewindEnd += StopRewind;
            _rewindEventChannel.OnRewindTick += OnRewindTick;
        }


        public void StartRewind()
        {
             
            _isReversing = true;
 
            _skinnedMeshRenderer.enabled = false;
            _skinnedMeshRendererGhost.enabled = true;
            characterAnimationRewinder.OnRewindStart();
            transformRewinder.OnRewindStart();
        }
        
        public void StopRewind()
        {
            _isReversing = false;
 
            _skinnedMeshRenderer.enabled = true;
            _skinnedMeshRendererGhost.enabled = false;
            
            characterAnimationRewinder.OnRewindStop();
            transformRewinder.OnRewindStop();
        }
        
        private void OnRewindTick(float time)
        {
            transformRewinder.ApplyRewind(time);
        }

 
    }
}