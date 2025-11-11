using System;
using System.Collections;
using System.Collections.Generic;
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
            
             DestroyGhosts();
            
            for (int i = 0; i < FrameData.Count; i += _visualizeEveryNthFrame)
            {
                var frame = FrameData[i];
                var ghost = Instantiate(_ghostPrefab, transform.position, transform.rotation);
                var animator = ghost.GetComponent<Animator>();
                ApplyFrameDataToGhost(frame, animator);
                
                 var snapshot = BakeGhostSnapshot(ghost);

                Destroy(ghost);

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
            StartCoroutine(FadeOutAllSnapshotsCoroutine(.5f,DestroyGhosts ));
        }
        
        private void DestroyGhosts()
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


        private List<MeshRenderer> _snapshotRenderers = new();
        
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
            
            meshRenderer.materials = newMaterials;

            _snapshotRenderers.Add(meshRenderer);


            return snapshot;
        }


        private IEnumerator FadeOutAllSnapshotsCoroutine(float fadeDuration, Action onComplete)
        {
            if (_snapshotRenderers.Count == 0)
                yield break;

            float elapsed = 0f;
            float startAlpha = _traceSettings.StartingAlpha;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                float currentAlpha = Mathf.Lerp(startAlpha, 0f, t);

                // update all materials
                for (int rendererIndex = 0; rendererIndex < _snapshotRenderers.Count; rendererIndex++)
                {
                    var renderer = _snapshotRenderers[rendererIndex];
                    if (renderer == null)
                        continue;

                    var materials = renderer.materials;
                    for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                    {
                        var material = materials[materialIndex];
                        if (material == null)
                            continue;

                        material.SetFloat("_Alpha", currentAlpha);
                    }
                }

                yield return null;
            }
          
            for (int rendererIndex = 0; rendererIndex < _snapshotRenderers.Count; rendererIndex++)
            {
                var renderer = _snapshotRenderers[rendererIndex];
                if (renderer == null)
                    continue;

                var materials = renderer.materials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    var material = materials[materialIndex];
                    if (material == null)
                        continue;

                    material.SetFloat("_Alpha", 0f);
                }

            }
            onComplete?.Invoke();
        }

        private class GhostInstance
        {
            public FrameData Frame;
            public GameObject GameObject;
            public Renderer Renderer;
        }

    }
}