using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using StarterAssets.ScriptableData;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace ReverseRelated
{
    public class PlayablesController: MonoBehaviour
    {
        
    [SerializeField] private Animator animator;
    [SerializeField] private AnimStatesDataStorageSO _dataStorageSO;
    
    private PlayableGraph graph;
    private AnimationPlayableOutput playableOutput;
    private AnimationMixerPlayable masterMixer;

    private Dictionary<int, PlayableState> _playableStates = new Dictionary<int, PlayableState>();

    [SerializeField]  private PlayableState _activeState = null;
    private void Awake()
    {
        _dataStorageSO.FillDictionary();
    }

    void Start()
    {
        CreateGraph();
       
        GeneratePlayableStates();
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
            playableState.index = index;
            
            _playableStates.Add(playableState.hashName, playableState);
            
            graph.Connect(mixer, 0, masterMixer, index);
            index++;
        }
    }
    
     


    private float blendDuration = .22f;
    [SerializeField]   private PlayableState _oldState = null;

    
    
    
    public async void PlayState(int hash, float endTime, CancellationToken token)
    {
        if(token.IsCancellationRequested) return;
       
        if(_activeState != null && _activeState.hashName == hash ) return;

        if (!_playableStates.ContainsKey(hash))
        {
            Debug.Log($"[PlayablesController] _playableStates doesnt contain hash {hash}");
            return;
        }

        if (_activeState != null)
        {
            _oldState = _activeState.Clone();
         //   _oldState.clipPlayables[_oldState.defaultAnimIndex].Pause();
        }

     
        
        _activeState = _playableStates[hash];
     //    masterMixer.SetInputWeight(_activeState.index, 1f);
 
    
        
        foreach (var pare in _playableStates)
        {
            if (pare.Key == _activeState.hashName) continue;
            if (_oldState == null || pare.Key == _oldState.hashName ) continue;
            masterMixer.SetInputWeight(pare.Value.index, 0f);
            
            foreach (var clip in pare.Value.clipPlayables)
            {
                  clip.Pause();
            }
        }

        _activeState.clipPlayables[_activeState.defaultAnimIndex].SetTime(endTime);
        _activeState.stateMixer.SetInputWeight(_activeState.defaultAnimIndex, 1f);
        _activeState.clipPlayables[_activeState.defaultAnimIndex].SetSpeed(-Time.timeScale);
        _activeState.clipPlayables[_activeState.defaultAnimIndex].Play();
        
        if (_oldState == null|| _oldState.hashName== 0)
        {
            Debug.Log($"[PlayablesController] _oldState == null|| _oldState.hashName== 0");
            masterMixer.SetInputWeight(_activeState.index, 1f);
          
            return;
        }
         
        BlendStates(_oldState, _activeState, blendDuration, token);
    }
    
    private async void BlendStates(PlayableState oldState, PlayableState newState, float blendDuration, CancellationToken token)
    {
        float timeDEBUG = Time.time;
       Debug.Log($"[PlayablesController] BlendStates, oldState hashName: {oldState.hashName}, newState hashName: {newState.hashName}");
    //   oldState.clipPlayables[oldState.defaultAnimIndex].Pause();
        float elapsedTime = 0f;
        while (elapsedTime < blendDuration)
        {
            if(token.IsCancellationRequested) return;
            
            float t = elapsedTime / blendDuration;
            masterMixer.SetInputWeight(oldState.index, Mathf.Lerp(1f, 0f, t));
            masterMixer.SetInputWeight(newState.index, Mathf.Lerp(0f, 1f, t));
            elapsedTime += Time.deltaTime;
            await UniTask.Yield();
        }
        oldState.clipPlayables[oldState.defaultAnimIndex].Pause();
        masterMixer.SetInputWeight(oldState.index, 0f);
        masterMixer.SetInputWeight(newState.index, 1f);
        
        Debug.Log($"[PlayablesController] BlendStates, END {Time.time - timeDEBUG}");
    }

    


    public void StopPLay()
    {
        _oldState = null;
        _activeState = null;
        foreach (var pare in _playableStates)
        {
           masterMixer.SetInputWeight(pare.Value.index, 0f);
           foreach (var val in pare.Value.clipPlayables)
           {
               val.Pause();
           }
        }
    }


    }
}