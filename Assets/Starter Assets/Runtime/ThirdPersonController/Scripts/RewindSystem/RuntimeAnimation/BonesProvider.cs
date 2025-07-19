using System.Collections.Generic;
using UnityEngine;

namespace RewindSystem.RuntimeAnimation
{
    public class BonesProvider
    {
        public  Dictionary<HumanBodyBones, Transform> BoneMap => _boneMap;
        private Dictionary<HumanBodyBones, Transform> _boneMap;
        
        public  BonesProvider(Animator animator)
        {
            var allBones = (HumanBodyBones[])System.Enum.GetValues(typeof(HumanBodyBones));
            _boneMap = new();

            foreach (var bone in allBones)
            {
                if (bone == HumanBodyBones.LastBone) continue; // skip sentinel
                var t = animator.GetBoneTransform(bone);
                if (t != null)
                {
                    _boneMap[bone] = t;
                }
            }
        }
    }
}