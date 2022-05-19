using System;
using UnityEngine;

public class SimpleAnimatorBehaviour : StateMachineBehaviour
{
    public event Action<float> onProgress;

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        onProgress?.Invoke(stateInfo.normalizedTime);
    }
}
