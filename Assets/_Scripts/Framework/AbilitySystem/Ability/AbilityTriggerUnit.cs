using UnityEngine;

namespace GAS {
    public abstract class AbilityTriggerUnit : ScriptableObject {
        public abstract TaskStatus TryTrigger(AbilityComponentContext abilityComponentContext);
        public abstract AbilityTriggerUnit Clone();
    }
}