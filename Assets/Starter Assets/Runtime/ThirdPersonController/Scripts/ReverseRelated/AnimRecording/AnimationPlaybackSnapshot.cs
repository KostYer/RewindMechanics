using System;
using System.Collections.Generic;

namespace ReverseRelated.AnimRecording
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

}