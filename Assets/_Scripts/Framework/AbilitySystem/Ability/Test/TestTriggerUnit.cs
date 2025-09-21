using UnityEngine;

namespace AbilitySystem {
    [CreateAssetMenu(menuName = "Ability/Test/Trigger Unit", fileName = "TestTriggerUnit")]
    public class TestTriggerUnit : AbilityTriggerUnit {
        float lastTriggerTime = -10;
        public override TaskStatus TryTrigger(AbilityComponentContext abilityComponentContext) { 
            if(Time.time - lastTriggerTime > 5) {
                lastTriggerTime = Time.time;
                return TaskStatus.Suceeded;
            } else { 
                return TaskStatus.Failed;
            }
        }
    }
}
