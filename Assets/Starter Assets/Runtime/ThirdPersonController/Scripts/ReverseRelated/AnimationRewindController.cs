using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ReverseRelated.AnimRecording;
using StarterAssets.ScriptableData;
using UnityEngine;
using UnityEngine.Animations;

namespace ReverseRelated
{
    public class AnimationRewindController: MonoBehaviour
    {
        [SerializeField] private Animator animator;
      
        private bool IsRewinding = false;
        
        private float rewindStartTime;
        private float rewindDuration;
        private float rewindEndTime;
        private int currentSnapshotIndex;
  
        
        public AnimStatesDataStorageSO _dataStorageSO;
        
        private List<AnimationClipPlayable> _animationClipPlayablesCurrent = new();
        
        
        [SerializeField] private AnimationRecorder animationRecorder;
        [SerializeField] private PlayablesController playablesController;
       
        
        [SerializeField] private RuntimeAnimatorController  _animationController;

        private string _currentSnapshotName;
        
        
        private Rewinder _rewinder;

        public void Init(Rewinder rewinder)
        {
            _rewinder = rewinder;
        }

        private void Start()
        {
            _animationController = animator.runtimeAnimatorController;
        }

        public void OnRewindStart()
        {
            IsRewinding = true;
            animationRecorder.OnRewindStart();
            animator.runtimeAnimatorController = null;
            rewindStartTime = Time.time;
       
        //    var targetHash = animationRecorder.AnimSnapshots[GetCurrentSnapshotIndex()].stateHash;
            
            
        //    playablesController.PlayState(targetHash);
            rewindCTS = new CancellationTokenSource();
            RewindLoop().Forget(); // Fire and forget the UniTask
        }

        public void OnRewindStop()
        {
            Debug.Log($"[OnRewindStopOnRewindStop]");
            IsRewinding = false;
            animator.runtimeAnimatorController = _animationController;
            animationRecorder.OnRewindStop();
            
            playablesController.StopPLay();

            if (rewindCTS != null && !rewindCTS.IsCancellationRequested)
            {
                rewindCTS.Cancel();
            }
           
            rewindCTS.Dispose();
            
            
        }

        CancellationTokenSource rewindCTS = new CancellationTokenSource();
    
        
        private async UniTaskVoid RewindLoop()
        {
            while (IsRewinding)
            {
                
                float targetTime = _rewinder.GetRewindTargetTime();

                // Reached the beginning of recorded data
                if (targetTime <= animationRecorder.AnimSnapshots[0].realTimeStarted)
                {
                    playablesController.PlayState(animationRecorder.AnimSnapshots[0].stateHash, 0f, rewindCTS.Token);
                    break;
                }

                int index = GetCurrentSnapshotIndex(targetTime);
                var snapshot = animationRecorder.AnimSnapshots[index];

                _currentSnapshotName = _dataStorageSO.StatesDictionary[snapshot.stateHash].Name;

               // float rewindDuration = animationRecorder.AnimSnapshots[index].realTimeStarted - animationRecorder.AnimSnapshots[index - 1].realTimeStarted;
                //float rewindDuration = animationRecorder.AnimSnapshots[index].realTimeStarted - animationRecorder.AnimSnap
                
                float t = Mathf.InverseLerp(
                    snapshot.realTimeStarted + rewindDuration,  //  newer
                    snapshot.realTimeStarted,                  //   older
                    targetTime
                );
                float normalizedTime = Mathf.Lerp(snapshot.normalEndTime, snapshot.normalStartTime, t);
                
                
                playablesController.PlayState(snapshot.stateHash, snapshot.normalEndTime, rewindCTS.Token);

                await UniTask.Yield(); // Wait for next frame
            }
        }

        #region Rewinding

        
        
        
        /*private float GetRewindTargetTime()
        {
            float elapsed = Time.time - rewindStartTime;
            return rewindStartTime - elapsed;
        }*/
        
        private int GetCurrentSnapshotIndex(float targTime)
        {
             
            for (int i = animationRecorder.AnimSnapshots.Count -1; i > 0; i--)
            { 
                var snapshot = animationRecorder.AnimSnapshots[i];
                if (targTime >= snapshot.realTimeStarted)
                {
                    return i;
                }
            }
            return 0;
        }

        #endregion Rewinding
        
        
        
        private int fontSize = 24;               // Font size for better visibility
        private Color textColor = Color.white;   // Color of the text
        private GUIStyle textStyle;
        
        void OnGUI()
        {  
            textStyle = new GUIStyle();
            textStyle.fontSize = fontSize;
            textStyle.normal.textColor = textColor;
            
            textStyle.alignment = TextAnchor.UpperRight;
            float margin = 10f;
            float margin2 = 25f;
            Rect textRect2 = new Rect(margin, margin2, Screen.width - 2 * margin, Screen.height - 2 * margin2);
            textStyle.normal.textColor = IsRewinding ? Color.red : Color.white;
            GUI.Label(textRect2, "rev: "+ IsRewinding, textStyle);
             
            float margin3 = 75;
            Rect textRect3 = new Rect(margin, margin3, Screen.width - 2 * margin, Screen.height - 2 * margin3);
            textStyle.normal.textColor = Color.yellow;
            GUI.Label(textRect3, _currentSnapshotName, textStyle);
        }
        
    }
}