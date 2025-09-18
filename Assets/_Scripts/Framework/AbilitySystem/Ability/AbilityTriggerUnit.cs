using UnityEngine;

namespace AbilitySystem {
    public abstract class AbilityTriggerUnit : ScriptableObject {
        public abstract TaskStatus TryTrigger(AbilityComponentContext abilityComponentContext);
    }
}