using System;
using System.Collections;
using System.Collections.Generic;
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
      ///  public bool isLast;
        public List<BlendChange> blendChanges = new(); 
        
        public AnimationPlaybackSnapshot Clone()
        {
            return new AnimationPlaybackSnapshot
            {
                stateHash = this.stateHash,
                normalStartTime = this.normalStartTime,
                normalEndTime = this.normalEndTime,
                realTimeStarted = this.realTimeStarted,
             //   isLast = this.isLast, 
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
        [SerializeField] private AnimationPlaybackSnapshot _currentSnapshot;
        
        public bool IsRewinding = false;
        
        private float rewindStartTime;
        private float rewindDuration;
        private float rewindEndTime;
        private int currentSnapshotIndex;
        private AnimationClipPlayable clipPlayable;
        
        public AnimStatesDataStorageSO _dataStorageSO;

        [SerializeField] private bool IsGameBlendTree;
        
        private List<AnimationClipPlayable> _animationClipPlayablesCurrent = new();
        
        private PlayableGraph graph;
        private AnimationPlayableOutput playableOutput;
        private AnimationMixerPlayable playableMixer;
        
        [SerializeField] private RuntimeAnimatorController  _animationController;

        private void Awake()
        {
            _dataStorageSO.FillDictionary();
            _animationController = animator.runtimeAnimatorController;
        }

        void Start()
        {
            graph = PlayableGraph.Create();
            playableOutput = AnimationPlayableOutput.Create(graph, "Animation", animator);
            playableMixer = AnimationMixerPlayable.Create(graph, 3);
        }

        private void OnValidate()
        {
            animator = GetComponent<Animator>();
        }
        

        public void OnRewindStart()
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            RecordAnimSnapshot(stateInfo, true);
            animator.runtimeAnimatorController = null;
            IsRewinding = true;
            
            AnimateRewind();
        }
        
        public void StopRewind()
        {
            Debug.Log($"[StopRewind]");
            IsRewinding = false;

            CreateAnimSnapshot(animator.GetCurrentAnimatorStateInfo(0));
            animator.runtimeAnimatorController = _animationController;
        }
        
        private void CreateAnimationPlayablesOLD(int nameHash)
        {
            // Destroy previous mixer if any
            if (playableMixer.IsValid())
                playableMixer.Destroy();

            var clips = _dataStorageSO.StatesDictionary[nameHash].Clips;

            // Create a new mixer with correct number of inputs
            playableMixer = AnimationMixerPlayable.Create(graph, clips.Count, true);

            _animationClipPlayablesCurrent.Clear();
            for (int i = 0; i < clips.Count; i++)
            {
                var clipPlayableCurrent = AnimationClipPlayable.Create(graph, clips[i]);
                clipPlayableCurrent.SetSpeed(-1f);
                Debug.Log($"[CreateAnimationPlayables] clip {i}, clipName {clips[i].name}, len: {clips[i].length}");
                graph.Connect(clipPlayableCurrent, 0, playableMixer, i);
                _animationClipPlayablesCurrent.Add(clipPlayableCurrent);
            }
        }
        
        private void CreateAnimationPlayables(int stateHash)
        {
            // Destroy previous mixer if any
            if (playableMixer.IsValid())
            {
                playableMixer.Destroy();
            }

            var clips = _dataStorageSO.StatesDictionary[stateHash].Clips;

            playableMixer = AnimationMixerPlayable.Create(graph, clips.Count, true);

            _animationClipPlayablesCurrent.Clear();

            for (int i = 0; i < clips.Count; i++)
            {
                var clipPlayable = AnimationClipPlayable.Create(graph, clips[i]);
                clipPlayable.SetSpeed(-1f);
                // Disconnect previous connections to this input slot if any
                if (graph.GetOutputCount() > i)
                {
                    graph.Disconnect(playableMixer, i);
                }

                graph.Connect(clipPlayable, 0, playableMixer, i);
              //  playableMixer.SetInputWeight(i, 0f);

                _animationClipPlayablesCurrent.Add(clipPlayable);
            }

            playableOutput.SetSourcePlayable(playableMixer);

            // Optionally reset mixer weights or pause clips initially
            for (int i = 0; i < _animationClipPlayablesCurrent.Count; i++)
            {
                 _animationClipPlayablesCurrent[i].Pause();
            //    playableMixer.SetInputWeight(i, 0f);
            }
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


        #region Rewind logic
        
        

        private void AnimateRewind()
        {
            if (_animSnapshots.Count == 0)
            {
                Debug.LogError($"[AnimateRewind] _animSnapshots.Count == 0");
                return;
            }
            
            // Rewind parameters
            rewindStartTime = Time.time;
            rewindEndTime = _animSnapshots[_animSnapshots.Count  -1].realTimeStarted;
            rewindDuration = rewindStartTime - rewindEndTime;
            
            var snapshot = _animSnapshots[GetCurrentSnapshotIndex()];
            //  var snapshot = GetCurrentSnapshot(targetTime);
            if (snapshot.stateHash == 0)
            {
                Debug.LogError($"[AnimateRewind] snapshot.stateHash == 0");
            }
            
            CreateAnimationPlayables(snapshot.stateHash);
         

            // Start from the end of the list
            currentSnapshotIndex = GetCurrentSnapshotIndex();
            currentPlayableStateHash = snapshot.stateHash;
            
            playableOutput.SetSourcePlayable(playableMixer);
       
            graph.Play();
         
            
            StartCoroutine(RewindCoroutine());
        }

        private float blendValue;
        private int currentPlayableStateHash;

      

        private float GetRewindTargetTime()
        {
            float elapsed = Time.time - rewindStartTime;
            return rewindStartTime - elapsed;
        }
         
        private int GetCurrentSnapshotIndex()
        {
            var targTime = GetRewindTargetTime();
            for (int i = _animSnapshots.Count -1; i > 0; i--)
            { 
            var snapshot = _animSnapshots[i];
             if (targTime >= snapshot.realTimeStarted)
             {
                 return i;
             }
            }
            return 0;
        }


        private bool isRewindBlendTree;
        private IEnumerator RewindCoroutineOld()
        {
            while (IsRewinding && currentSnapshotIndex >= 0)
            {
                // STEP 1: Calculate how far back we should be in real time
                float targetTime = GetRewindTargetTime();
              
                // STEP 2: Grab current snapshot
                
                currentSnapshotIndex = GetCurrentSnapshotIndex();
                var snapshot = _animSnapshots[ currentSnapshotIndex];
               
                currentlyRewindingState = _dataStorageSO.StatesDictionary[snapshot.stateHash].Name;
               
                isRewindBlendTree = _dataStorageSO.StatesDictionary[snapshot.stateHash].IsTree;
               
              
                if (snapshot.stateHash != currentPlayableStateHash)
                {
                    Debug.Log($"[snapshot.stateHash] snapshot changed, current snapshot: {snapshot.stateHash}");
                    CreateAnimationPlayables(snapshot.stateHash);  
                } 
             
                currentPlayableStateHash = snapshot.stateHash;
                
                
                // STEP 3: Check if we're "before" this snapshot → move to previous
                if (targetTime < snapshot.realTimeStarted)
                {
                    currentSnapshotIndex--;
                    continue;
                }

                // STEP 4: Calculate blend value (for blend tree)
             //   float blendValue = GetBlendValueAtTime(snapshot.blendChanges, targetTime);

                // STEP 5: Apply blend weights to mixer (if needed)
                if (isRewindBlendTree)
                {
                    float blendValue = GetBlendValueAtTime(snapshot.blendChanges, targetTime);
                    ApplyBlendWeights(blendValue, snapshot.stateHash);
                }
                
                else
                {
                    for (int i = 0; i < playableMixer.GetInputCount(); i++)
                        playableMixer.SetInputWeight(i, i == 0 ? 1f : 0f); 
                }
               

                // STEP 6: Calculate normalized animation time between start and end of this snapshot
                float t = Mathf.InverseLerp(
                    snapshot.realTimeStarted + rewindDuration,  //  newer
                    snapshot.realTimeStarted,                  //   older
                    targetTime
                );
                float normalizedTime = Mathf.Lerp(snapshot.normalEndTime, snapshot.normalStartTime, t);

                // STEP 7: Apply to each playable clip
                foreach (var playable in _animationClipPlayablesCurrent)
                {
                    float clipLength = playable.GetAnimationClip().length;
                    playable.SetTime(normalizedTime * clipLength);
                    playable.Pause();
                }

                yield return null;
            }
 
        }
        
        private IEnumerator RewindCoroutine()
        {
            while (IsRewinding && currentSnapshotIndex >= 0)
            {
                float targetTime = GetRewindTargetTime();
                currentSnapshotIndex = GetCurrentSnapshotIndex();

                var snapshot = _animSnapshots[currentSnapshotIndex];
                currentlyRewindingState = _dataStorageSO.StatesDictionary[snapshot.stateHash].Name;
                isRewindBlendTree = _dataStorageSO.StatesDictionary[snapshot.stateHash].IsTree;

                if (snapshot.stateHash != currentPlayableStateHash)
                {
                    CreateAnimationPlayables(snapshot.stateHash);
                    currentPlayableStateHash = snapshot.stateHash;
                }

                if (targetTime < snapshot.realTimeStarted)
                {
                    currentSnapshotIndex--;
                    continue;
                }

                if (isRewindBlendTree)
                {
                    float blendValue = GetBlendValueAtTime(snapshot.blendChanges, targetTime);
                    ApplyBlendWeights(blendValue, snapshot.stateHash);
                }
                else
                {
                    // Set weight 1 only for first clip, 0 for others
                    for (int i = 0; i < playableMixer.GetInputCount(); i++)
                        playableMixer.SetInputWeight(i, i == 0 ? 1f : 0f);
                }

                float t = Mathf.InverseLerp(
                    snapshot.realTimeStarted + rewindDuration,
                    snapshot.realTimeStarted,
                    targetTime
                );
                float normalizedTime = Mathf.Lerp(snapshot.normalEndTime, snapshot.normalStartTime, t);

                for (int i = 0; i < _animationClipPlayablesCurrent.Count; i++)
                {
                    var playable = _animationClipPlayablesCurrent[i];
                    float clipLength = playable.GetAnimationClip().length;
                    playable.SetTime(normalizedTime * clipLength);
                    playable.Pause();
                }

                yield return null;
            }
        }


        private float GetBlendValueAtTime(List<BlendChange> blendChanges, float rewindTime)
        {
            if (blendChanges == null || blendChanges.Count == 0)
                return 0f;

            for (int i = blendChanges.Count - 1; i > 0; i--)
            {
                if (rewindTime >= blendChanges[i].realTime)
                {
                    var from = blendChanges[i];
                    var to = blendChanges[i - 1];

                    float t = Mathf.InverseLerp(from.realTime, to.realTime, rewindTime);

                    return Mathf.Lerp(from.blendValue, to.blendValue, t);
                } 
            }
            return blendChanges[0].blendValue; // fallback
        }
        
        private void ApplyBlendWeights(float blendValue, int stateHash)
        {
            Debug.Log($"[ApplyBlendWeights]");
            var thresholds = _dataStorageSO.StatesDictionary[stateHash].Thresholds;

            float totalWeight = 0f;
            for (int i = 0; i < _animationClipPlayablesCurrent.Count; i++)
            {
                float weight = Compute1DBlendWeight(blendValue, thresholds, i);
                playableMixer.SetInputWeight(i, weight);
                
                
                totalWeight += weight;
                
                
                ////DEBUG
                if (weight >= .6f)
                {
                    currentlyRewindingClip = _animationClipPlayablesCurrent[i].GetAnimationClip().name;
                }
            }

            // Normalize (optional)
            if (totalWeight > 0f)
            {
                for (int i = 0; i < _animationClipPlayablesCurrent.Count; i++)
                {
                    float w = playableMixer.GetInputWeight(i);
                    playableMixer.SetInputWeight(i, w / totalWeight);
                }
            }
        }
        
        private float Compute1DBlendWeight(float value, List<float> thresholds, int index)
        {
            if (thresholds.Count == 1) return 1f;

            if (index == 0)
            {
                float next = thresholds[1];
                return Mathf.Clamp01(1f - (value - thresholds[0]) / (next - thresholds[0]));
            }
            else if (index == thresholds.Count - 1)
            {
                float prev = thresholds[index - 1];
                return Mathf.Clamp01((value - prev) / (thresholds[index] - prev));
            }
            else
            {
                float prev = thresholds[index - 1];
                float next = thresholds[index + 1];
                if (value < thresholds[index])
                    return Mathf.Clamp01((value - prev) / (thresholds[index] - prev));
                else
                    return Mathf.Clamp01(1f - (value - thresholds[index]) / (next - thresholds[index]));
            }
        }


        #endregion Rewind logic


        #region Recording logic

        private void CreateAnimSnapshot(AnimatorStateInfo stateInfo)
        {
            _currentSnapshot = new AnimationPlaybackSnapshot();
            _currentSnapshot.stateHash = stateInfo.shortNameHash;
            _currentSnapshot.normalStartTime = stateInfo.normalizedTime;
            _currentSnapshot.realTimeStarted = Time.time;
       ///     _currentSnapshot.isLast = false;
            _currentSnapshot.blendChanges = new List<BlendChange>();
        }
         
        private void RecordAnimSnapshot(AnimatorStateInfo stateInfo, bool isLast = false)
        {
            Debug.Log($"[AnimationRewindPlayer] RecordAnimSnapshot isLast {isLast}");
            if(_currentSnapshot == null) return;
            var snapshot = _currentSnapshot.Clone();
            snapshot.normalEndTime = stateInfo.normalizedTime;
        //    snapshot.isLast = isLast;
            _currentSnapshot = null;
        if (snapshot.realTimeStarted == 0f) return;
        _animSnapshots.Add(snapshot);
          
        }

        ///blend trees recordings
        private List<float> thresholds = new();

        private int lastRecordedRegionIndex = 0;
        private float lastRecordedSpeed = 0;
        
        void Update()
        {
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

        #endregion Recording logic
        
        
        #region States change
        
        public void OnAnimationStateEnter(AnimatorStateInfo stateInfo)
        {
            if (IsRewinding) return;
            
            if (playableMixer.IsValid())
                playableMixer.Destroy();
            
            
            IsGameBlendTree = _dataStorageSO.StatesDictionary[stateInfo.shortNameHash].IsTree;
            CreateAnimSnapshot(stateInfo);
            Debug.Log($"[AnimationRewindPlayer] OnAnimationStateEnter {stateInfo}");
            
        //    IsGameBlendTree = IsStateBlendTree(stateInfo.shortNameHash);

            if (IsGameBlendTree)
            {
                thresholds = _dataStorageSO.StatesDictionary[stateInfo.shortNameHash].Thresholds;
            }
            
            Debug.Log($"[AnimationRewindPlayer] IsCurrentState blend tree {IsGameBlendTree} (stateHash {stateInfo.shortNameHash})");
           // CreateAnimationPlayables(stateInfo.shortNameHash);
        }

        public void OnAnimationStateExit(AnimatorStateInfo stateInfo)
        {
            if(IsRewinding) return;
            RecordAnimSnapshot(stateInfo);
            Debug.Log($"[AnimationRewindPlayer] OnAnimationStateExit {stateInfo}");
        }
        #endregion States change
    
        public int fontSize = 24;               // Font size for better visibility
        public Color textColor = Color.white;   // Color of the text
        private GUIStyle textStyle;

        private string currentlyRewindingState = "tst1";
        private string currentlyRewindingClip = "tst2";
        
        void OnGUI()
        {
            // Initialize the GUIStyle when the script is enabled
            textStyle = new GUIStyle();
            textStyle.fontSize = fontSize;
            textStyle.normal.textColor = textColor;
            // Align text to top-right
            textStyle.alignment = TextAnchor.UpperRight;
            
            // X position: Start from Screen.width and subtract enough space for the text itself.
            // Y position: Start from a small margin from the top (e.g., 10 pixels).
            // Width: Give it enough width to contain your text.
            // Height: Give it enough height.

            // A good practice is to create a Rect that spans the entire top-right corner,
            // then use TextAnchor.UpperRight for alignment within that Rect.
            float margin = 10f;
            Rect textRect = new Rect(margin, margin, Screen.width - 2 * margin, Screen.height - 2 * margin);

            // Draw the label
            GUI.Label(textRect, animator.GetFloat("Speed").ToString(), textStyle);
            
            
            float margin2 = 25f;
            Rect textRect2 = new Rect(margin, margin2, Screen.width - 2 * margin, Screen.height - 2 * margin2);
            textStyle.normal.textColor = IsRewinding? Color.red : Color.white;
            
            GUI.Label(textRect2, IsRewinding.ToString(), textStyle);
            
           if(!IsRewinding) return;
            
            float margin3 = 60f;
            Rect textRect3 = new Rect(margin, margin3, Screen.width - 2 * margin, Screen.height - 2 * margin3);
            textStyle.normal.textColor = Color.red;
            GUI.Label(textRect3, currentlyRewindingState, textStyle);
            
            float margin4 = 90f;
            Rect textRect4 = new Rect(margin, margin4, Screen.width - 2 * margin, Screen.height - 2 * margin4);
            textStyle.normal.textColor = Color.red;
         //   GUI.Label(textRect4, currentlyRewindingClip, textStyle);
            
            float margin5 = 130f;
            Rect textRect5 = new Rect(margin, margin5, Screen.width - 2 * margin, Screen.height - 2 * margin5);
            textStyle.normal.textColor = Color.blue;
            GUI.Label(textRect5, isRewindBlendTree.ToString(), textStyle);
        }
    }
}