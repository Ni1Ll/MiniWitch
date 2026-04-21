using UnityEngine;
using Invector.vCharacterController;

public class LockMovementSMB : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var controller = animator.GetComponent<vThirdPersonController>();
        if (controller == null) return;

        controller.lockMovement = true;
        controller.lockRotation = true;
        controller.lockActions = true; // 👈 ДОБАВИЛИ
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var controller = animator.GetComponent<vThirdPersonController>();
        if (controller == null) return;

        controller.lockMovement = false;
        controller.lockRotation = false;
        controller.lockActions = false; // 👈
    }
}