using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace StarterAssets
{
    
    public class PlayableAnimationTest: MonoBehaviour
    {
        [SerializeField] private AnimationClip animationClip;
        [SerializeField] private AnimationClip animationClip2;
        [SerializeField] private AnimationClip animationClip3;
        [SerializeField] private AnimationClip animationClip4;
      
        private PlayableGraph graph;
         
        private AnimationClipPlayable clipPlayableA;
        private AnimationClipPlayable clipPlayableB;
        private bool playingSecond = false;

        private AnimationMixerPlayable mixer;
        
        private AnimationPlayableOutput output;
        
        private bool isBlending = false;
        private float blendTimer = 0f;
        
        public float blendDuration = 0.5f;
        void Start()
        {
            graph = PlayableGraph.Create("CrossfadeGraph");

            // 1. Create the output
            var output = AnimationPlayableOutput.Create(graph, "Animation", GetComponent<Animator>());

            // 2. Create playables
            clipPlayableA = AnimationClipPlayable.Create(graph, animationClip);
            clipPlayableB = AnimationClipPlayable.Create(graph, animationClip2);
            clipPlayableB.SetTime(0);
            clipPlayableB.Pause();
            
            clipPlayableA.SetSpeed(-1);
            clipPlayableB.SetSpeed(-1);

            // 3. Create mixer and connect both clips
            mixer = AnimationMixerPlayable.Create(graph, 2);
            graph.Connect(clipPlayableA, 0, mixer, 0);
            graph.Connect(clipPlayableB, 0, mixer, 1);

            // 4. Initially play only A
            mixer.SetInputWeight(0, 1f);
            mixer.SetInputWeight(1, 0f);

            output.SetSourcePlayable(mixer);
            graph.Play();
        }
        
        void Update()
        {
            double timeA = clipPlayableA.GetTime();

            if (!isBlending && timeA >= animationClip.length - blendDuration)
            {
                // Start blend
                clipPlayableB.Play();
                blendTimer = 0f;
                isBlending = true;
            }

            if (isBlending)
            {
                blendTimer += Time.deltaTime;
                float t = Mathf.Clamp01(blendTimer / blendDuration);

                // Lerp weights
                mixer.SetInputWeight(0, 1f - t);
                mixer.SetInputWeight(1, t);
            }
        }
        
    }
}