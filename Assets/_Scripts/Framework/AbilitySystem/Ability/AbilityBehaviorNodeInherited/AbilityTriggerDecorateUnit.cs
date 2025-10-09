using System.Collections.Generic;

namespace GAS {
    public abstract class AbilityTriggerDecorateUnit : AbilityTriggerUnit {
        protected List<AbilityTriggerUnit> triggerUnits;
        public void OnBuild(List<AbilityTriggerUnit> triggerUnits) {
            this.triggerUnits = triggerUnits;
        }
    }
}