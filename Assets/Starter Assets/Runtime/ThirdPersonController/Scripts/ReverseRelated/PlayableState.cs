using System;
using System.Collections.Generic;
using UnityEngine.Animations;

namespace ReverseRelated
{
    [Serializable]
    public class PlayableState
    {
        public int hashName;
        public int index;
        public AnimationMixerPlayable stateMixer;
        public List<AnimationClipPlayable> clipPlayables = default;

        public int defaultAnimIndex => clipPlayables.Count <= 1 ? 0 : 2;
        
        public PlayableState Clone()
        {
            // For a class, you create a new instance first.
            PlayableState clone = new PlayableState
            {
                hashName = this.hashName,
                index = this.index,
                stateMixer = this.stateMixer, // This is a shallow copy of the struct (copying the handle)
                // This performs a deep copy of the LIST, populating it with copies of the AnimationClipPlayable structs.
                // Since AnimationClipPlayable is a struct, its values are copied.
                clipPlayables = this.clipPlayables
            };
        
            return clone;
        }
        

    }
}