using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Recorders;
using StarterAssets.ScriptableObjects;
using UnityEngine;

namespace RewindSystem.Traces
{
    public class CharacterTraceVisualizer: MonoBehaviour
    {
        [SerializeField] private CharacterAnimationRewinder _animationRewinder;
        [SerializeField] private GameObject _ghostPrefab;
   
        
        [SerializeField] private RewindEventChannelSO _eventChannel;
        
        
        
        [SerializeField] private Material _ghostBaseMaterial;
        [SerializeField] private TraceSettingsSO _traceSettings;
        
        
        
        private int _visualizeEveryNthFrame = 7;

        private float _traceTimeLen = .5f;

        
        private readonly List<GhostInstance> _ghosts = new();

        private class GhostInstance
        {
            public FrameData Frame;
            public GameObject GameObject;
            public Renderer Renderer;
        }
        

        private void Update()
        {
            if (Input.GetKey(KeyCode.G))
            {
                VisualizeDebug();
            }
        }

        private List<FrameData> FrameData => _animationRewinder.CharacterAnimationRecorder.RecordedFrames;
        
        
        [ContextMenu("VisualizeDebug")]
        public void VisualizeDebug()
        {
            if (FrameData == null || FrameData.Count == 0)
            {
                Debug.Log($"[CharacterTraceVisualizer] FrameData is empty");
                return;
            }

            Debug.Log($"[CharacterTraceVisualizer] FrameData.Count {FrameData.Count}");

            ClearGhosts();
            
            
            for (int i = 0; i < FrameData.Count; i += _visualizeEveryNthFrame)
            {
                var frame = FrameData[i];
                var ghost = Instantiate(_ghostPrefab, transform.position, transform.rotation);
                var animator = ghost.GetComponent<Animator>();
                ApplyFrameDataToGhost(frame, animator);
                
                RegisterGhost(frame, ghost);
                
                // NEW: bake this posed ghost into a static snapshot
                var snapshot = BakeGhostSnapshot(ghost);

                // if you don't need the rig anymore, kill it
            //    Destroy(ghost);

                // and register the baked snapshot for time-based hiding
                RegisterGhost(frame, snapshot);
            }

        }
        
        
        void ApplyFrameDataToGhost(FrameData frame, Animator animator)
        {
            animator.transform.position = frame.worldPosition;
            animator.transform.rotation = frame.worldRotation;
            
            foreach (var kvp in frame.bones)
            {
                Transform bone = animator.GetBoneTransform(kvp.Key);
                if (bone == null)
                    continue;

                var boneFrame = kvp.Value;
                bone.localPosition = boneFrame.localPosition;
                bone.localRotation = boneFrame.localRotation;
            }
        }
        
       
        
        
      

        private void RegisterGhost(FrameData frame, GameObject ghost)
        {
            var renderer = ghost.GetComponentInChildren<Renderer>();

            _ghosts.Add(new GhostInstance
            {
                Frame = frame,
                GameObject = ghost,
                Renderer = renderer
            });
        }

        private void ClearGhosts()
        {
            for (int i = 0; i < _ghosts.Count; i++)
            {
                if (_ghosts[i].GameObject != null)
                    Destroy(_ghosts[i].GameObject);
            }

            _ghosts.Clear();
        }

       
     
        private void OnRewindTick(float targetTime)
        {
            for (int i = 0; i < _ghosts.Count; i++)
            {
                var ghost = _ghosts[i];
                if (ghost.Renderer == null)
                    continue;

                bool shouldBeVisible = ghost.Frame.time <= targetTime 
                                      &&  ghost.Frame.time >= targetTime - _traceTimeLen;
                
                
                if (ghost.Renderer.enabled != shouldBeVisible)
                    ghost.Renderer.enabled = shouldBeVisible;
            }
        }

      

        public void OnStartRewind()
        {
            _eventChannel.OnRewindTick += OnRewindTick;
            VisualizeDebug();
        }

        public void OnStopRewind()
        {
            _eventChannel.OnRewindTick -= OnRewindTick;
            ClearGhosts();
        }
        
        
        
        private GameObject BakeGhostSnapshot(GameObject ghost)
        {
            var skinned = ghost.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinned == null)
            {
                Debug.LogError("[CharacterTraceVisualizer] No SkinnedMeshRenderer found on ghost");
                return ghost; // fallback: just return original
            }

            var bakedMesh = new Mesh();
            skinned.BakeMesh(bakedMesh);

            var snapshot = new GameObject("BakedMeshSnapshot");
            snapshot.transform.position = skinned.transform.position;
            snapshot.transform.rotation = skinned.transform.rotation;
            snapshot.transform.localScale = skinned.transform.lossyScale;

            var meshFilter = snapshot.AddComponent<MeshFilter>();
            var meshRenderer = snapshot.AddComponent<MeshRenderer>();

            meshFilter.mesh = bakedMesh;

            // copy ghost material per submesh
            var newMaterials = new Material[bakedMesh.subMeshCount];
            for (int i = 0; i < bakedMesh.subMeshCount; i++)
            {
                var mat = new Material(_ghostBaseMaterial);
                mat.SetFloat("_Alpha", _traceSettings.StartingAlpha);
                mat.SetColor("_GhostColor", _traceSettings.TraceColor);
                newMaterials[i] = mat;
            }

            meshRenderer.materials = newMaterials;

            // reuse your existing fade logic
            var traceUnit = new TraceUnit(snapshot);
            /*traceUnit
                .FadeOutAsync(_traceSettings.TraceUnitLifetime, _traceSettings.StartingAlpha)
                .Forget();*/

            return snapshot;
        }
        
        
        
        
        
        
        
        
    }
}