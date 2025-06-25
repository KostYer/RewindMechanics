using System.Collections.Generic;
using StarterAssets.ScriptableData;
using UnityEditor.Animations;
using UnityEngine;

namespace ReverseRelated.EditorHelpers
{
    [ExecuteInEditMode]
    public class AnimationStateDataExtractor: MonoBehaviour
    {
        public AnimatorController animatorController;
        private List<AnimationStateData> statesData = new List<AnimationStateData>();

        public AnimStatesDataStorageSO _statesStorage = default;
        

        [ContextMenu("Get Blend Tree Clips and Thresholds")]
        void GetBlendTreeData()
        {
            if (animatorController == null)
            {
                Debug.LogError("[AnimationStateDataExtractor] Please assign an Animator Controller.");
                return;
            }

            statesData.Clear();
            // Iterate through all layers in the Animator Controller
            foreach (AnimatorControllerLayer layer in animatorController.layers)
            {
                Debug.Log($"[AnimationStateDataExtractor] Checking Layer: {layer.name}");

                // Get the state machine for the current layer
                AnimatorStateMachine stateMachine = layer.stateMachine;

                // Iterate through all states in the state machine
                foreach (ChildAnimatorState childState in stateMachine.states)
                {
                    AnimatorState state = childState.state;
                    Debug.Log($"[AnimationStateDataExtractor] Checking State: {state.name}");

                    // Check if the state's motion is a BlendTree
                    if (state.motion is BlendTree blendTree)
                    {
                        Debug.Log($"[AnimationStateDataExtractor]  Found Blend Tree: {blendTree.name} (Type: {blendTree.blendType})");

                        List<AnimationClip> clips = new List<AnimationClip>();
                        List<float> thresholds = new List<float>();
                        AnimationStateData data = new AnimationStateData();
                        
                        // For 1D Blend Trees, thresholds are directly available
                        if (blendTree.blendType == BlendTreeType.Simple1D ||
                            blendTree.blendType ==
                            BlendTreeType.Direct) // Direct blend also uses thresholds in ChildMotion
                        {
                          
                            for (int i = 0; i < blendTree.children.Length; i++)
                            {
                                ChildMotion childMotion = blendTree.children[i];

                                if (childMotion.motion is AnimationClip clip)
                                {
                                    clips.Add(clip);
                                    thresholds.Add(childMotion.threshold);
                                   
                                    
                                    Debug.Log($"[AnimationStateDataExtractor] stateName {state.name} Clip: {clip.name}, Threshold: {childMotion.threshold}");
                                }
                                
                            }
                        }
                       
                        else
                        {
                            Debug.Log($"[AnimationStateDataExtractor] Unsupported Blend Tree Type: {blendTree.blendType}");
                        }

                        data.HameHash = Animator.StringToHash(state.name);
                        data.Name = state.name;
                        data.Clips = clips;
                        data.Thresholds = thresholds;
                        data.IsTree = true;
                        statesData.Add(data);

                        // Now 'clips' and 'thresholds' (or positions for 2D) contain the data
                        // You can do whatever you need with them here.
                        Debug.Log($"[AnimationStateDataExtractor] Total clips found in {blendTree.name}: {clips.Count}");
                    }
                    
                    else if (state.motion is AnimationClip animationClip)
                    {
                        AnimationStateData data = new AnimationStateData();
                        data.HameHash = Animator.StringToHash(state.name);
                        data.Name = state.name;
                        data.Clips = new List<AnimationClip> { animationClip };
                        data.Thresholds = null;
                        data.IsTree = false;
                        statesData.Add(data);
                    }
                    
                }
                
                _statesStorage.AddStateData(statesData);
            }
        }


    }
}