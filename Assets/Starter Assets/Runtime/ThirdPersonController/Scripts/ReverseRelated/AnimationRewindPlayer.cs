using System;
using System.Collections;
using System.Collections.Generic;
using ReverseRelated.AnimBehaviour;
using StarterAssets.ScriptableData;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace ReverseRelated
{
    [Serializable]
    public class AnimationPlaybackSnapshot
    {
        public int stateHash;  
        public float realTimeStarted;         
        public float normalStartTime; 
        public float normalEndTime;
        public bool isLast;
        public List<BlendChange> blendChanges = new(); 
        
        public AnimationPlaybackSnapshot Clone()
        {
            return new AnimationPlaybackSnapshot
            {
                stateHash = this.stateHash,
                normalStartTime = this.normalStartTime,
                normalEndTime = this.normalEndTime,
                realTimeStarted = this.realTimeStarted,
                isLast = this.isLast, 
                blendChanges = this.blendChanges
            };
        }
    }
    
    [Serializable]
    public struct BlendChange
    {
        public float realTime;      // e.g., Time.time
        public float normalizedTime; // e.g., animator.GetCurrentAnimatorStateInfo(0).normalizedTime
        public float blendValue;     // e.g., animator.GetFloat("Speed")
    }
    
    public class AnimationRewindPlayer: MonoBehaviour
    {
        [SerializeField] private Animator animator;
        
        [SerializeField] private List<AnimationPlaybackSnapshot> _animSnapshots = new ();
        
        private PlayableGraph graph;

        [SerializeField] private AnimationPlaybackSnapshot _currentSnapshot;
        
        public bool IsRewinding = false;
        
         
        
        private float rewindStartTime;
        private float rewindDuration;
        private float rewindEndTime;
        private int currentSnapshotIndex;
        private AnimationClipPlayable clipPlayable;
        
        
        public AnimStatesDataStorageSO _dataStorageSO;

        [SerializeField] private bool IsInBlendState = true;

        private void Awake()
        {
            _dataStorageSO.FillDictionary();
        }

        void Start()
        {
            graph = PlayableGraph.Create();
            var playableOutput = AnimationPlayableOutput.Create(graph, "Animation", animator);

            /*var clipPlayable = AnimationClipPlayable.Create(graph, clip);
            playableOutput.SetSourcePlayable(clipPlayable);

            
            clipPlayable = AnimationClipPlayable.Create(graph, clip);
            playableOutput.SetSourcePlayable(clipPlayable);
            
            graph.Play();*/
        }

        private void OnValidate()
        {
            animator = GetComponent<Animator>();
        }


        public void OnRewindStart()
        {
            IsRewinding = true;
            animator.enabled = false;
          //  Debug.Log($"[AnimationRewindPlayer] OnRewindStart");
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"[AnimationRewindPlayer] OnRewindStart {stateInfo.fullPathHash}");
            RecordAnimSnapshot(stateInfo, true);
            AnimateRewind();
        }

        private void AnimateRewind()
        {
            if (_animSnapshots.Count == 0) return;

            // Rewind parameters
            rewindStartTime = Time.time;
            rewindEndTime = _animSnapshots[_animSnapshots.Count - 1].realTimeStarted;
            rewindDuration = rewindStartTime - rewindEndTime;

            // Start from the end of the list
            currentSnapshotIndex = _animSnapshots.Count - 1;

            StartCoroutine(RewindCoroutine());
        }
        
        private IEnumerator RewindCoroutine()
        {
            while (IsRewinding && currentSnapshotIndex >= 0)
            {
                float rewindTime = rewindStartTime - Time.time;

                // Clamp to available range
                if (currentSnapshotIndex <= 0) break;

                var snapshot = _animSnapshots[currentSnapshotIndex];

                if (rewindTime < snapshot.realTimeStarted)
                {
                    currentSnapshotIndex--;
                    continue;
                }

                float t = Mathf.InverseLerp(snapshot.realTimeStarted, snapshot.realTimeStarted + rewindDuration, rewindTime);
                float normalizedTime = Mathf.Lerp(snapshot.normalEndTime, snapshot.normalStartTime, t);

              //  clipPlayable.SetTime(normalizedTime * clip.length);
               // clipPlayable.Pause(); // So it stays on frame

                yield return null;
            }

            StopRewind();
        }

        public void StopRewind()
        {
            IsRewinding = false;
            animator.enabled = true;
        }

        public void OnAnimationStateEnter(AnimatorStateInfo stateInfo)
        {
            if(IsRewinding) return;
            CreateAnimSnapshot(stateInfo);
            Debug.Log($"[AnimationRewindPlayer] OnAnimationStateEnter {stateInfo}");
            
            IsInBlendState= IsStateBlendTree(stateInfo.shortNameHash);
            
            thresholds = _dataStorageSO.StatesDictionary[stateInfo.shortNameHash].Thresholds;
            
            Debug.Log($"[AnimationRewindPlayer] IsCurrentState blend tree {IsInBlendState} (stateHash {stateInfo.shortNameHash})");
        }

        private bool IsStateBlendTree(int hash)
        {

            if (!_dataStorageSO.StatesDictionary.ContainsKey(hash))
            {
                Debug.LogError($"[AnimationRewindPlayer] IsStateBlendTree. hash {hash} is not in the dictionary");
                return false;
            }

            return _dataStorageSO.StatesDictionary[hash].IsTree;

        }

        public void OnAnimationStateExit(AnimatorStateInfo stateInfo)
        {
            if(IsRewinding) return;
            RecordAnimSnapshot(stateInfo);
            Debug.Log($"[AnimationRewindPlayer] OnAnimationStateExit {stateInfo}");
        }

        private void CreateAnimSnapshot(AnimatorStateInfo stateInfo)
        {
            _currentSnapshot = new AnimationPlaybackSnapshot();
            _currentSnapshot.stateHash = stateInfo.shortNameHash;
            _currentSnapshot.normalStartTime = stateInfo.normalizedTime;
            _currentSnapshot.realTimeStarted = Time.time;
            _currentSnapshot.isLast = false;
        }

        private void RecordAnimSnapshot(AnimatorStateInfo stateInfo, bool isLast = false)
        {
            Debug.Log($"[AnimationRewindPlayer] RecordAnimSnapshot isLast {isLast}");
            var snapshot = _currentSnapshot.Clone();
            snapshot.normalEndTime = stateInfo.normalizedTime;
            snapshot.isLast = isLast;

            _animSnapshots.Add(snapshot);
             _currentSnapshot = null;
        }



        ///blend trees recordings
        private List<float> thresholds = new();

        private int lastRecordedRegionIndex = 0;
        
        void Update()
        {
            if (!IsRewinding && IsInBlendState)
            {
                float speed = animator.GetFloat("Speed");
                int regionIndex = GetCurrentRegionIndex(speed, thresholds);

                if (regionIndex != lastRecordedRegionIndex)
                {
                    // Speed crossed a threshold → record snapshot
                    var blendChange = new BlendChange
                    {
                        realTime = Time.time,
                        normalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime,
                        blendValue = speed
                    };

                    _currentSnapshot.blendChanges.Add(blendChange);
                    lastRecordedRegionIndex = regionIndex;
                }
            }
        }
        
        
        int GetCurrentRegionIndex(float speed, List<float> thresholds)
        {
            for (int i = thresholds.Count - 1; i >= 0; i--)
            {
                if (speed >= thresholds[i])
                    return i;
            }
            return 0; // default to lowest
        }
    }
}