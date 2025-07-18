using System.Collections.Generic;
using UnityEngine;

namespace Recorders
{
    
    [System.Serializable]
    public class BoneFrameData
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
    }

    [System.Serializable]
    public class FrameData
    {
        public float time;
        public Dictionary<HumanBodyBones, BoneFrameData> bones = new();
    }
    
    public class CharacterAnimationRecorder: MonoBehaviour
    {
        public Animator animator;
    

        [SerializeField] private List<FrameData> recordedFrames = new();
        private HumanBodyBones[] allBones;
        private Dictionary<HumanBodyBones, Transform> boneMap;
    

        private AnimationClip _clip = default;
      

        private bool _isRewinding;

        void Start()
        {
            // Cache all valid Humanoid bones
            allBones = (HumanBodyBones[])System.Enum.GetValues(typeof(HumanBodyBones));
            boneMap = new();

            foreach (var bone in allBones)
            {
                if (bone == HumanBodyBones.LastBone) continue; // skip sentinel
                var t = animator.GetBoneTransform(bone);
                if (t != null)
                {
                    boneMap[bone] = t;
                }
            }
        }

        void LateUpdate()
        {
            if (_isRewinding) return;

            RecordFrame();
        }


        private void RecordFrame()
        {
            var frame = new FrameData();
            frame.time = Time.time;
            
            
            
            foreach (var kvp in boneMap)
            {
                var bone = kvp.Key;
                var t = kvp.Value;

                frame.bones[bone] = new BoneFrameData
                {
                    localPosition = t.localPosition,
                    localRotation = t.localRotation
                };
            }

            recordedFrames.Add(frame);
        }
 
        public void OnRewindStart()
        {
            _isRewinding = true;
        }

        public void OnRewindStop()
        {
            _isRewinding = false;
            recordedFrames.Clear();
        }
        
        public AnimationClip CreateAnimationClipFromFrames()
        {
            _clip = new AnimationClip();
            _clip.name = "RewindClip";
            _clip.legacy = false;

            float frameRate = 45f; // adjust if different
            float dt = 1f / frameRate;

            foreach (var bone in boneMap)
            {
                var humanBone = bone.Key;
                var transform = bone.Value;

               
                
                string bonePath = GetRelativePath(transform,  animator.transform); // we'll define this
                
                Debug.Log("Bone path: " + bonePath);

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
                        Debug.Log($"[RawFrameRecorder] doesnt contain data for {humanBone}");
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

                _clip.SetCurve(bonePath, typeof(Transform), "localPosition.x", posX);
                _clip.SetCurve(bonePath, typeof(Transform), "localPosition.y", posY);
                _clip.SetCurve(bonePath, typeof(Transform), "localPosition.z", posZ);

                _clip.SetCurve(bonePath, typeof(Transform), "localRotation.x", rotX);
                _clip.SetCurve(bonePath, typeof(Transform), "localRotation.y", rotY);
                _clip.SetCurve(bonePath, typeof(Transform), "localRotation.z", rotZ);
                _clip.SetCurve(bonePath, typeof(Transform), "localRotation.w", rotW);
            }

            _clip.EnsureQuaternionContinuity();
            return _clip;
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