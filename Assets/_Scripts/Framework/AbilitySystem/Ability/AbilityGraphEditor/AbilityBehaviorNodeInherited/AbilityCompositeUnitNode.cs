using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace AbilitySystem.Editor.AbilityEditor {
    [CreateNodeMenu("Ability/BehaviorUnit/Composite")]
    public class AbilityCompositeUnitNode : AbilityBehaviorUnitNode {
        [Input(backingValue = ShowBackingValue.Never,
            connectionType = ConnectionType.Override,
            typeConstraint = TypeConstraint.Inherited)]
        public AbilityBehaviorUnitNode abilityBehaviorUnitNode;

        [Output(backingValue = ShowBackingValue.Never,
            connectionType = ConnectionType.Multiple,
            typeConstraint = TypeConstraint.Strict)]
        public AbilityBehaviorUnitNode childrenNodes;

        [field: SerializeField] public AbilityCompositeUnit abilityCompositeUnit;

        // Use this for initialization
        protected override void Init() {
            base.Init();
        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            return null; // Replace this
        }

        public override AbilityBehaviorUnit Build() {
            //TODO：这里的构建有问题，并且Composite节点没有排序
            AbilityBehaviorUnit unit = abilityCompositeUnit.Clone();
            List<AbilityBehaviorUnit> childs = new List<AbilityBehaviorUnit>();
            NodePort outPort = GetOutputPort(nameof(childrenNodes));
            foreach(var conn in outPort.GetConnections()) { 
                childs.Add((conn.node as AbilityBehaviorUnitNode).Build());
            }
            unit.OnBuild(childs);
            return unit;
        }
    }
}
