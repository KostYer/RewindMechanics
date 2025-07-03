using System;
using System.Collections.Generic;
using ReverseRelated;
using StarterAssets.ScriptableData;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class GraphLearnLab: MonoBehaviour
{
    [SerializeField] private Animator animator;
  //  [SerializeField] private Slider _slider;
    [SerializeField] private AnimStatesDataStorageSO _dataStorageSO;
    
    private PlayableGraph graph;
    private AnimationPlayableOutput playableOutput;
    private AnimationMixerPlayable masterMixer;

    private Dictionary<int, PlayableState> _playableStates = new Dictionary<int, PlayableState>();

    private void Awake()
    {
        _dataStorageSO.FillDictionary();
    }

    void Start()
    {
        CreateGraph();
       
        GeneratePlayableStates();
 
        Debug.Log($"[class] values: {_dataStorageSO.StatesDictionary.Values.Count}");
        masterMixer.SetInputWeight(2, 1f);
        _playableStates[132228977].stateMixer.SetInputWeight(0, 1f);
        graph.Play();
    }

    
    private void CreateGraph()
    {
        graph = PlayableGraph.Create();
        playableOutput = AnimationPlayableOutput.Create(graph, "Animation", animator);
        masterMixer = AnimationMixerPlayable.Create(graph, _dataStorageSO.StatesDictionary.Values.Count, true);
       
        playableOutput.SetSourcePlayable(masterMixer);
    }

    private void GeneratePlayableStates()
    {
        int index = 0;
        foreach (var kvp in _dataStorageSO.StatesDictionary)
        {
            var key = kvp.Key;
            var value = kvp.Value;

            var playableState = new PlayableState();
            
            var mixer = AnimationMixerPlayable.Create(graph, value.Clips.Count, true);
            Debug.Log($"[GeneratePlayableStates] index: {index}, clipsCount {value.Clips.Count} ");

            var clipPlayables = new List<AnimationClipPlayable>();
 
            for (int i = 0; i < value.Clips.Count; i++)
            {
                var clipPlayable = AnimationClipPlayable.Create(graph, value.Clips[i]);
                clipPlayable.SetSpeed(-1f);
                
                graph.Connect(clipPlayable, 0, mixer, i);
                mixer.SetInputWeight(i, 0f);
                clipPlayables.Add(clipPlayable);
            }

            playableState.hashName = key;
            playableState.stateMixer = mixer;
            playableState.clipPlayables = clipPlayables;
            
            _playableStates.Add(playableState.hashName, playableState);
            
            graph.Connect(mixer, 0, masterMixer, index);
            index++;
        }
      
    }

    
    private void OnSliderDrag(float value)
    {
     //   playableMixer.SetInputWeight(0, 1f - value);
     //   playableMixer.SetInputWeight(1, value);
    }


  

   

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log($"[GraphLearnLab] Input.GetKeyDown(KeyCode.Z");
           
          
        }
    }
}
