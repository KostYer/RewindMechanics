using RewindSystem.Traces;
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


        [SerializeField] private CharacterTraceVisualizer _trace;

        private float _targetTime;

        [SerializeField] private bool _isRewindEnabled = true;
        private void Awake()
        {
            _skinnedMeshRendererGhost.enabled = false;
            
            if(!_isRewindEnabled) return;
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

            _trace.OnStartRewind();
        }
        
        private void StopRewind()
        {
            _skinnedMeshRenderer.enabled = true;
            _skinnedMeshRendererGhost.enabled = false;
            
            characterAnimationRewinder.ApplyAnimationState(_targetTime); //MUST be before    characterAnimationRewinder.OnRewindStop();
            
            characterAnimationRewinder.OnRewindStop();
            transformRewinder.OnRewindStop();
            _traceDrawer.StopDraw();
            
            _trace.OnStopRewind();
        }
        
        private void OnRewindTick(float time)
        {
            transformRewinder.ApplyRewind(time);
            _targetTime = time;
        }
    }
}