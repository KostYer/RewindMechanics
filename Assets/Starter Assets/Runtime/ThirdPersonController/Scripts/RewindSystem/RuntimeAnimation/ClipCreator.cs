using System.Collections.Generic;
using Recorders;
using UnityEngine;

namespace RewindSystem.RuntimeAnimation
{
    public class ClipCreator
    {
        public AnimationClip CreateAnimationClipFromFrames(Animator animator,List<FrameData> recordedFrames, Dictionary<HumanBodyBones, Transform> boneMap)
        {
           var clip = new AnimationClip();
           clip.name = "RewindClip";
           clip.legacy = false;

            foreach (var bone in boneMap)
            {
                var humanBone = bone.Key;
                var transform = bone.Value;
                string bonePath = GetRelativePath(transform,  animator.transform); 

                var posX = new AnimationCurve();
                var posY = new AnimationCurve();
                var posZ = new AnimationCurve();

                var rotX = new AnimationCurve();
                var rotY = new AnimationCurve();
                var rotZ = new AnimationCurve();
                var rotW = new AnimationCurve();
                
                float baseTime = recordedFrames[0].time;

                for (int i = 0; i < recordedFrames.Count; i++)
                {
                    if (!recordedFrames[i].bones.TryGetValue(humanBone, out var boneData))
                    {
                        Debug.LogError($"[RawFrameRecorder] doesnt contain data for {humanBone}");
                        continue;
                    }
                    float time = recordedFrames[i].time - baseTime;

                    Vector3 pos = boneData.localPosition;
                    Quaternion rot = boneData.localRotation;

                    posX.AddKey(time, pos.x);
                    posY.AddKey(time, pos.y);
                    posZ.AddKey(time, pos.z);

                    rotX.AddKey(time, rot.x);
                    rotY.AddKey(time, rot.y);
                    rotZ.AddKey(time, rot.z);
                    rotW.AddKey(time, rot.w);
                }

                clip.SetCurve(bonePath, typeof(Transform), "localPosition.x", posX);
                clip.SetCurve(bonePath, typeof(Transform), "localPosition.y", posY);
                clip.SetCurve(bonePath, typeof(Transform), "localPosition.z", posZ);

                clip.SetCurve(bonePath, typeof(Transform), "localRotation.x", rotX);
                clip.SetCurve(bonePath, typeof(Transform), "localRotation.y", rotY);
                clip.SetCurve(bonePath, typeof(Transform), "localRotation.z", rotZ);
                clip.SetCurve(bonePath, typeof(Transform), "localRotation.w", rotW);
            }

            clip.EnsureQuaternionContinuity();
            return clip;
        }
        
        private string GetRelativePath(Transform target, Transform root)
        {
            if (target == null || root == null)
                return null;

            if (target == root)
                return "";

            if (target.parent == null)
                throw new System.Exception($"[GetRelativePath] Target {target.name} is not a child of root {root.name}");

            string parentPath = GetRelativePath(target.parent, root);
            if (string.IsNullOrEmpty(parentPath))
                return target.name;

            return parentPath + "/" + target.name;
        }
    }
}