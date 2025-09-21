using UnityEngine;

namespace AbilitySystem {
    [CreateAssetMenu(menuName = "Ability/Test/Decorate Unit", fileName = "TestDecorateUnit")]
    public class TestDecorateUnit : AbilityDecorateUnit {
        public override void OnTriggered(AbilityRuntimeContext abilityRuntimeContext) { }
        public override TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext) => TaskStatus.Suceeded;
        public override TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext, bool allEffectFinished) => TaskStatus.Suceeded;
        public override TaskStatus OnInterrupt(InteruptionContext intreruptionContext) => TaskStatus.Suceeded;
        public override AbilityBehaviorUnit Clone() {
            var inst = CreateInstance<TestDecorateUnit>();
            inst.HeadInfo = this.HeadInfo;
            return inst;
        }
    }
}
