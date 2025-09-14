using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbilitySystem{
    internal class AbilityExcutionTask {
        private Ability Ability;
        private AbilityRuntimeContext runtimeContext;
        public void OnTriggered(AbilityComponentContext abilityComponentContext) {
            
        }

        public TaskStatus OnUpdate(AbilityComponentContext abilityComponentContext) {
            return default;
        }

        public TaskStatus OnExit(AbilityComponentContext abilityComponentContext) { 
            return default;
        }

        public TaskStatus OnInterrupted(IIntreruptionContext intreruptionContext) {
            return default;
        }
    }
}
