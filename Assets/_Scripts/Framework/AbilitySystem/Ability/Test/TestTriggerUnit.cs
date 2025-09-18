using UnityEngine;

namespace AbilitySystem {
    [CreateAssetMenu(menuName = "Ability/Test/Trigger Unit", fileName = "TestTriggerUnit")]
    public class TestTriggerUnit : AbilityTriggerUnit {
        public override TaskStatus TryTrigger(AbilityComponentContext abilityComponentContext) => TaskStatus.Suceeded;
    }
}
