using System.Collections.Generic;
using ReverseRelated;
using UnityEngine;

namespace StarterAssets.ScriptableData
{
    [CreateAssetMenu(fileName = "AnimStatesDataStorageSO", menuName = "SO/AnimStatesDataStorageSO")]
    public class AnimStatesDataStorageSO: ScriptableObject
    {
        [SerializeField] private List<AnimationStateData> _statesData = new List<AnimationStateData>();
        public List<AnimationStateData> StatesData => _statesData;
        
        private Dictionary<int, AnimationStateData> _statesDictionary = new ();
        public Dictionary<int, AnimationStateData> StatesDictionary => _statesDictionary;
        
         
        public void AddStateData(List<AnimationStateData> sd)
        {
            _statesData.Clear();
        
            
            _statesData = sd;
          
        }

        public void FillDictionary()
        {
            _statesDictionary.Clear();
            
            for (int i = 0; i < _statesData.Count; i++)
            {
                _statesDictionary.Add(Animator.StringToHash(_statesData[i].Name), _statesData[i]);  
            }
        }

    }
}