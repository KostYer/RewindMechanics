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
    
        
        private void Awake()
        {
            _skinnedMeshRendererGhost.enabled = false;
            _rewindEventChannel.OnRewindStart += StartRewind;
            _rewindEventChannel.OnRewindEnd += StopRewind;
            _rewindEventChannel.OnRewindTick += OnRewindTick;
        }

        public void StartRewind()
        {
            _skinnedMeshRenderer.enabled = false;
            _skinnedMeshRendererGhost.enabled = true;
            characterAnimationRewinder.OnRewindStart();
            transformRewinder.OnRewindStart();
        }
        
        public void StopRewind()
        {
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