using UnityEngine;
using XNode;

namespace GAS.Editor.AbilityEditor {
    [CreateNodeMenu("Ability/Trigger")]
    [NodeTint("#5c7080")]
    public class AbilityTriggerUnitNode : Node {
        // 父：AbilityNode.TriggerPort 或 任一 Decorator 的输出
        [Input(backingValue = ShowBackingValue.Never,
               connectionType = ConnectionType.Override,
               typeConstraint = TypeConstraint.Strict)]
        public AbilityTriggerUnitNode AbilityPort;

        [field: SerializeField] public AbilityTriggerUnit TriggerUnit { get; set; }

        public AbilityTriggerUnit Build() => TriggerUnit.Clone();

        public override object GetValue(NodePort port) => this;

        public override void OnCreateConnection(NodePort from,NodePort to) {
            base.OnCreateConnection(from,to);
            // 仅允许被 Ability.TriggerPort 或 Decorator 的输出连接到本节点的输入
            if(to.node == this && to.fieldName == nameof(AbilityPort)) {
                var parent = from.node;
                bool ok = parent is AbilityNode || parent is AbilityTriggerDecorateUnitNode;
                if(!ok) {
                    Debug.LogError("Trigger 节点只能出现在 Ability.TriggerPort 延伸的树上（父必须是 Ability 或 Trigger Decorator）。");
                    to.Disconnect(from);
                }
            }

            // 触发节点自身不应作为父节点输出连接到任何子节点（它是叶子）。本类未定义输出端口，正常不会发生。
        }
    }
}