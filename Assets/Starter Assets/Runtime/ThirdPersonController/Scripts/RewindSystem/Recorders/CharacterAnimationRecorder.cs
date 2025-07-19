using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using RewindSystem.RuntimeAnimation;
using StarterAssets.Interfaces;
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
    
    public class CharacterAnimationRecorder: IRecorder<FrameData>
    {
        
        private BonesProvider _bonesProvider;
        private List<FrameData> _recordedFrames = new();
        private Dictionary<HumanBodyBones, Transform> boneMap => _bonesProvider.BoneMap;
 
        private bool _isRewinding;
        private CancellationTokenSource _tokenSource;
      
        public float MaxDuration { get; private set; }

        public CharacterAnimationRecorder(BonesProvider bp, float maxDur)
        {
            _bonesProvider = bp;
            MaxDuration = maxDur;
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

            _recordedFrames.Add(frame);
        }
 
        public void StartRecording()
        {
            Clear();
            _tokenSource?.Cancel();  
            _tokenSource?.Dispose();
            _tokenSource = new CancellationTokenSource();

            RecordSnapshots(_tokenSource.Token).Forget();
        }

        public void StopRecording()
        {
            _tokenSource?.Cancel();  
            _tokenSource?.Dispose();
            _tokenSource = null;
           
        }

        public void Clear()
        {
            _recordedFrames.Clear();
        }

        public List<FrameData> GetSnapshots()
        {
            return _recordedFrames;
        }
      
        private async UniTaskVoid RecordSnapshots(CancellationToken token)
        {
            float totalRecordedTime = 5f;  

            while (!token.IsCancellationRequested)
            {
                RecordFrame();

                float timeNow = Time.time;
                while (_recordedFrames.Count > 0 && timeNow - _recordedFrames[0].time > totalRecordedTime)
                    _recordedFrames.RemoveAt(0);

                await UniTask.Yield(PlayerLoopTiming.PreLateUpdate, token);
            }
        }
    }
}