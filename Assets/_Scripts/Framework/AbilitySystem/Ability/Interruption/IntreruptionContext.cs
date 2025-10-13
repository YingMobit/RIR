using System.Collections.Generic;

namespace GAS {
    public struct InteruptionContext {
        public int SourceID;
        public int InteruptionPriority;
        public InteruptionContext(int interuptionPriority,int sourceID) {
            InteruptionPriority = interuptionPriority;
            SourceID = sourceID;
        }
    }

    public struct InteruptionHandler { 
        public List<AbilityRuntimeContext> abilityRuntimeContexts;
        public InteruptionHandler(List<AbilityRuntimeContext> contexts) { 
            abilityRuntimeContexts = contexts;
        }
    }
}
