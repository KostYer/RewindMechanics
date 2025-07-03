using System;
using ReverseRelated.AnimRecording;
using UnityEngine;

namespace ReverseRelated.AnimBehaviour
{
    public class AnimationStateTracker: StateMachineBehaviour
    {

        private AnimationRecorder controller;
        

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (controller == null)
            {
                controller =  animator.GetComponent<AnimationRecorder>();
            }

            

            //   var controller = animator.GetComponent<AnimationRecorder>();
         controller.OnAnimationStateEnter(stateInfo);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
       //   var controller = animator.GetComponent<AnimationRecorder>();
           controller.OnAnimationStateExit(stateInfo);
        }
       
    }
}