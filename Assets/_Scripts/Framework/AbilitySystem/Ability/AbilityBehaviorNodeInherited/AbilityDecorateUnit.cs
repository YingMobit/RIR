namespace AbilitySystem {
    public abstract class AbilityDecorateUnit : AbilityBehaviorUnit {
        public override TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext) {
            throw new System.NotImplementedException();
        }

        public override TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext,bool allEffectFinished) {
            throw new System.NotImplementedException();
        }

        public override TaskStatus OnInterrupt(IIntreruptionContext intreruptionContext) {
            throw new System.NotImplementedException();
        }

        public override void OnTriggered(AbilityRuntimeContext abilityRuntimeContext) {
            throw new System.NotImplementedException();
        }
    }
}