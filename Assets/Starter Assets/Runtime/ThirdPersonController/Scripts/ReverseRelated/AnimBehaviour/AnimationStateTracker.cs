using System;
using UnityEngine;

namespace ReverseRelated.AnimBehaviour
{
    public class AnimationStateTracker: StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        { 
            var controller = animator.GetComponent<AnimationRewindPlayer>();
            controller?.OnAnimationStateEnter(stateInfo);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
           var controller = animator.GetComponent<AnimationRewindPlayer>();
            controller?.OnAnimationStateExit(stateInfo);
        }
       
    }
}