using Sirenix.OdinInspector;
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

        [field: SerializeField] public AbilityCompositeUnit AbilityCompositeUnit { get; private set; }
        public override HeadInfo HeadInfo => AbilityCompositeUnit ? AbilityCompositeUnit.HeadInfo : default;
        protected override AbilityBehaviorUnit AbilityBehaviorUnit => AbilityCompositeUnit;

        // Use this for initialization
        protected override void Init() {
            base.Init();
        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            return null; // Replace this
        }

        public override AbilityBehaviorUnit Build() {
            AbilityBehaviorUnit unit = AbilityCompositeUnit.Clone();
            List<AbilityBehaviorUnit> childs = new List<AbilityBehaviorUnit>();
            NodePort outPort = GetOutputPort(nameof(childrenNodes));
            foreach(var conn in outPort.GetConnections()) { 
                childs.Add((conn.node as AbilityBehaviorUnitNode).Build());
            }
            InjectUnitNodeRefrence(unit);
            unit.OnBuild(childs,RuntimeToken);
            return unit;
        }

        public override int SetRuntimeToken(int token) {
            RuntimeToken = token;
            int nextToken = token + 1;
            NodePort outPort = GetOutputPort(nameof(childrenNodes));
            foreach(var conn in outPort.GetConnections()) {
                nextToken = (conn.node as AbilityBehaviorUnitNode).SetRuntimeToken(nextToken);
            }
            return nextToken;
        }

        public override void OnTokenDisplayed(int newToken) {
            Node node = GetInputPort(nameof(abilityBehaviorUnitNode)).Connection.node;
            if(node is AbilityBehaviorUnitNode behaviorNode) {
                behaviorNode.OnTokenDisplayed(newToken);
            } else if(node is AbilityEffectNode effectNode) {
                effectNode.OnBehaviorNodeDisplayedToken(newToken);
            }
        }
    }
}
