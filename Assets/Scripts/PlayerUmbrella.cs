using UnityEngine;

public class PlayerUmbrella : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var equipment = animator.GetComponent<PlayerEquipment>();
        if (equipment != null) equipment.ShowUmbrella();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var equipment = animator.GetComponent<PlayerEquipment>();
        if (equipment != null) equipment.HideUmbrella();
    }
}
