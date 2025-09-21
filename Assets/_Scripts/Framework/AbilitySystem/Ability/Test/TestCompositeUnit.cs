using UnityEngine;

namespace AbilitySystem {
    [CreateAssetMenu(menuName = "Ability/Test/Composite Unit", fileName = "TestCompositeUnit")]
    public class TestCompositeUnit : AbilityCompositeUnit {
        public override void OnTriggered(AbilityRuntimeContext abilityRuntimeContext) { }
        public override TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext) { 
            foreach(var child in Childs) { 
                child.OnExcute(abilityRuntimeContext);
            }
            return TaskStatus.Suceeded;
        }
        public override TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext, bool allEffectFinished) => TaskStatus.Suceeded;
        public override TaskStatus OnInterrupt(InteruptionContext intreruptionContext) => default;
        public override AbilityBehaviorUnit Clone() {
            var inst = CreateInstance<TestCompositeUnit>();
            inst.HeadInfo = this.HeadInfo;
            return inst;
        }
    }
}
