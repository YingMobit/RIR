using GAS;
using InputSystemNameSpace;
using UnityEngine;

public class InputTrigger : AbilityTriggerUnit {
    [SerializeField] InputTypeEnum inputType;

    public override AbilityTriggerUnit Clone() {
        var res = new InputTrigger();
        res.inputType = inputType;
        return res;
    }

    public override TaskStatus TryTrigger(AbilityComponentContext abilityComponentContext) {
        var input = abilityComponentContext.GlobalBlacboard.Get<FrameInputModel>(0);
        if(input.KeyCodeinputs.HasInputType(inputType)) {
            return TaskStatus.Suceeded;
        }
        return TaskStatus.Failed;
    }
}