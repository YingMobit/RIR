using System.Collections.Generic;

namespace AbilitySystem {
    public struct InteruptionContext {
        public int InteruptionPriority;
        public InteruptionContext(int interuptionPriority) {
            InteruptionPriority = interuptionPriority;
        }
    }

    public struct InteruptionHandler { 
        public List<AbilityRuntimeContext> abilityRuntimeContexts;
        public InteruptionHandler(List<AbilityRuntimeContext> contexts) { 
            abilityRuntimeContexts = contexts;
        }
    }
}
