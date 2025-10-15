using GAS;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBlackBoardBoolCheckTrigger",menuName = "GAS/Triggers/Charactor/BlackBoardBoolCheckTrigger",order =0)]
public class BlackBoardBoolCheckTrigger : AbilityTriggerUnit {
    [SerializeField] int BlackBoardValueID;
    public override AbilityTriggerUnit Clone() {
        return Instantiate(this);
    }

    public override TaskStatus TryTrigger(AbilityComponentContext abilityComponentContext) {
        if(abilityComponentContext.GlobalBlacboard.Get<bool>(BlackBoardValueID)) {
            return TaskStatus.Suceeded;
        } else {
            return TaskStatus.Failed;
        }
    }
}