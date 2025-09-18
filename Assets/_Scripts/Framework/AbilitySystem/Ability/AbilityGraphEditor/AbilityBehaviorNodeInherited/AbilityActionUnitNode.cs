using UnityEngine;
using XNode;

namespace AbilitySystem.Editor.AbilityEditor {
    [CreateNodeMenu("Ability/BehaviorUnit/Action")]
    public class AbilityActionUnitNode : AbilityBehaviorUnitNode {
        [Input(backingValue = ShowBackingValue.Never,
            connectionType = ConnectionType.Override,
            typeConstraint = TypeConstraint.Inherited,
            dynamicPortList = false)]
        public AbilityBehaviorUnitNode behaviorUnitNode;

        [field: SerializeField] public AbilityActionUnit AbilityActionUnit { get; private set; }

        // Use this for initialization
        protected override void Init() {
            base.Init();

        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            return null; // Replace this
        }

        public override AbilityBehaviorUnit Build() {
            AbilityBehaviorUnit unit = AbilityActionUnit.Clone();
            unit.OnBuild(null);
            return unit;
        }
    }
}
