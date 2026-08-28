using UnityEngine;

public class EquipItemOnState : StateMachineBehaviour
{
    public string itemName;
    private PlayerEquipment equipment;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        equipment = animator.GetComponent<PlayerEquipment>();
        equipment?.ShowItem(itemName);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        equipment?.HideItem(itemName);
    }
}