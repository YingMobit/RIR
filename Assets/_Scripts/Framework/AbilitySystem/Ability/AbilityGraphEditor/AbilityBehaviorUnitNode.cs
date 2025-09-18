using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

namespace AbilitySystem.Editor.AbilityEditor {
    [CreateNodeMenu("Ability/Behavior/Base (abstract)")]
    [NodeTint("#5c7080")]
    public abstract class AbilityBehaviorUnitNode : Node {
        [Header("Behavior Unit")]
        public HeadInfo HeadInfo;
        [field: SerializeField, ReadOnly] public int Order { get; private set; }
        public void SetOrder(AbilityBehaviorUnitNode node,int order) {
            Order = order;
        }

        public override void OnCreateConnection(NodePort from,NodePort to) {
            base.OnCreateConnection(from,to);
            if(from.node == this) {
                int index = 0;
                foreach(var port in from.GetConnections()) {
                    (port.node as AbilityBehaviorUnitNode).SetOrder(this,index++);
                }
            } else if(from.node.GetType() == typeof(AbilityEffectNode)) {
                Order = 0;
            }
        }

        public override void OnRemoveConnection(NodePort port) {
            base.OnRemoveConnection(port);
            if(port.IsOutput) {
                int index = 0;
                foreach(var connection in port.GetConnections()) {
                    (connection.node as AbilityBehaviorUnitNode).SetOrder(this,index++);
                }
            } else {
                Order = -1;
            }
        }

        public abstract AbilityBehaviorUnit Build();
    }
}
