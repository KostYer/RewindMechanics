using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReverseRelated
{
    [Serializable]
    public class AnimationStateData
    {
        public int HameHash;
        public string Name;
        public bool IsTree;
        public List<AnimationClip> Clips;
        public List<float> Thresholds;


    }
}