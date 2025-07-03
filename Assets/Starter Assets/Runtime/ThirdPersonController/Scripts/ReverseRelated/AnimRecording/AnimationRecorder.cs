using System.Collections.Generic;
using StarterAssets.ScriptableData;
using UnityEngine;

namespace ReverseRelated.AnimRecording
{
    public class AnimationRecorder: MonoBehaviour
    {
        [SerializeField]  private bool IsGameBlendTree;// => _dataStorageSO.StatesDictionary[_currentSnapshot.stateHash].IsTree;
        
        [SerializeField] private AnimStatesDataStorageSO _dataStorageSO;
        [SerializeField] private Animator animator;
        
        
        [SerializeField] private AnimationPlaybackSnapshot _currentSnapshot;
         private List<AnimationPlaybackSnapshot> _animSnapshots = new List<AnimationPlaybackSnapshot>();
        
        public List<AnimationPlaybackSnapshot> AnimSnapshots => _animSnapshots;

        private bool IsRewinding;


        public void OnAnimationStateEnter(AnimatorStateInfo stateInfo)
        {
            CreateAnimSnapshot(stateInfo);
        }
        
        public void OnAnimationStateExit(AnimatorStateInfo stateInfo)
        {
            RecordAnimSnapshot(stateInfo);
        }
        
        public void OnRewindStart()
        {
            IsRewinding = true;
            RecordAnimSnapshot(animator.GetCurrentAnimatorStateInfo(0));
        }
        public void OnRewindStop()
        {
            IsRewinding = false;
            
            CreateAnimSnapshot(animator.GetCurrentAnimatorStateInfo(0));
        }


        private void CreateAnimSnapshot(AnimatorStateInfo stateInfo)
        {
             
      //      Debug.Log($"[CreateAnimSnapshot] stateInfo name hash {stateInfo.shortNameHash}");
            IsGameBlendTree = _dataStorageSO.StatesDictionary[stateInfo.shortNameHash].IsTree;
            //IsGameBlendTree = _dataStorageSO.StatesDictionary[stateInfo
            _currentSnapshot = new AnimationPlaybackSnapshot();
            _currentSnapshot.stateHash = stateInfo.shortNameHash;
            _currentSnapshot.normalStartTime = stateInfo.normalizedTime;
            _currentSnapshot.realTimeStarted = Time.time;
            _currentSnapshot.blendChanges = new List<BlendChange>();
        }
         
        private void RecordAnimSnapshot(AnimatorStateInfo stateInfo, bool isLast = false)
        {
            
            var snapshot = _currentSnapshot.Clone();
            snapshot.normalEndTime = stateInfo.normalizedTime;
            _animSnapshots.Add(snapshot);
          
        }

        private int lastRecordedRegionIndex = 0;
        private float lastRecordedSpeed = 0;
        
        void Update()
        {
            return;
            const float SPEED_EPSILON = 0.05f; // small threshold
            
            if (!IsRewinding && IsGameBlendTree)
            {
                float speed = animator.GetFloat("Speed");
            
                if (Mathf.Abs(speed - lastRecordedSpeed) > SPEED_EPSILON)
                {
                    var blendChange = new BlendChange
                    {
                        realTime = Time.time,
                        normalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime,
                        blendValue = speed
                    };
                    lastRecordedSpeed = speed;
                    
                     if (_currentSnapshot != null)
                     _currentSnapshot.blendChanges.Add(blendChange);
                }
            }
        }
    }
}