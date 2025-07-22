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
        [SerializeField] private TraceDrawer _traceDrawer;

        private float _targetTime;
        private void Awake()
        {
            _skinnedMeshRendererGhost.enabled = false;
            _rewindEventChannel.OnRewindStart += StartRewind;
            _rewindEventChannel.OnRewindEnd += StopRewind;
            _rewindEventChannel.OnRewindTick += OnRewindTick;
        }

        private void StartRewind()
        {
            _skinnedMeshRenderer.enabled = false;
            _skinnedMeshRendererGhost.enabled = true;
            characterAnimationRewinder.OnRewindStart();
            transformRewinder.OnRewindStart();
            _traceDrawer.StartDraw();
        }
        
        private void StopRewind()
        {
            _skinnedMeshRenderer.enabled = true;
            _skinnedMeshRendererGhost.enabled = false;
            
            characterAnimationRewinder.ApplyAnimationState(_targetTime); //MUST be before    characterAnimationRewinder.OnRewindStop();
            
            characterAnimationRewinder.OnRewindStop();
            transformRewinder.OnRewindStop();
            _traceDrawer.StopDraw();
        }
        
        private void OnRewindTick(float time)
        {
            transformRewinder.ApplyRewind(time);
            _targetTime = time;
        }
    }
}