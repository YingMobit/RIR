using GAS;
using InputSystemNameSpace;
using UnityEngine;

[CreateAssetMenu(fileName = "InputTrigger",menuName = "GAS/Triggers/Charactor/InputTrigger",order =0)]
public class InputTrigger : AbilityTriggerUnit {
    [SerializeField] InputTypeEnum inputType;

    public override AbilityTriggerUnit Clone() {
        var res = new InputTrigger();
        res.inputType = inputType;
        return res;
    }

    public override TaskStatus TryTrigger(AbilityComponentContext abilityComponentContext) {
        var input = abilityComponentContext.GlobalBlacboard.Get<InputQueue>(AbilitySystem.INPUTID_IN_GLOBALBLACKBORAD);
        var inputData = input.PeekTail();
        if(inputData.KeyCodeinputs.HasAnyInputType(inputType)) {
            return TaskStatus.Suceeded;
        }
        return TaskStatus.Failed;
    }
}