using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace GAS.Editor.AbilityEditor {
    [CreateNodeMenu("Ability/Trigger Decorator")]
    [NodeTint("#6b8e23")]
    public class AbilityTriggerDecorateUnitNode : Node {
        // 父：AbilityNode.TriggerPort 或 上一个 Decorator
        [Input(backingValue = ShowBackingValue.Never,
               connectionType = ConnectionType.Override,
               typeConstraint = TypeConstraint.Strict)]
        public AbilityTriggerUnitNode AbilityPort;

        // 子：多个 Decorator 或 终结 Trigger
        [Output(backingValue = ShowBackingValue.Never,
                connectionType = ConnectionType.Multiple,
                typeConstraint = TypeConstraint.Strict)]
        public AbilityTriggerUnitNode Children;

        // 装饰器资源（在你的实现中负责组合子触发的逻辑）
        [field: SerializeField] public AbilityTriggerDecorateUnit abilityTriggerDecorator { get; private set; }

        public override object GetValue(NodePort port) => this;

        // 构建：收集所有子 Trigger/Decorator，装饰器不得为叶子
        public AbilityTriggerUnit Build() {
            var outPort = GetOutputPort(nameof(Children));
            if(outPort == null) {
                Debug.LogError("Trigger Decorator 缺少 Children 输出端口。");
                return null;
            }

            var childUnits = new List<AbilityTriggerUnit>();
            var res = abilityTriggerDecorator.Clone();
            foreach(var conn in outPort.GetConnections()) {
                if(conn?.node is AbilityTriggerUnitNode tNode) {
                    var tu = tNode.Build();
                    if(tu != null)
                        childUnits.Add(tu);
                } else if(conn?.node is AbilityTriggerDecorateUnitNode dNode) {
                    var du = dNode.Build();
                    if(du != null)
                        childUnits.Add(du);
                } else {
                    Debug.LogError("Trigger Decorator 的子节点必须是 Trigger 或 Trigger Decorator。");
                    return null;
                }
            }

            if(childUnits.Count == 0) {
                Debug.LogError("Trigger Decorator 不能作为叶子节点：必须至少连接一个子 Trigger/Decorator。");
                return null;
            }

            (res as AbilityTriggerDecorateUnit).OnBuild(childUnits);
            return res;
            
        }

        public override void OnCreateConnection(NodePort from,NodePort to) {
            base.OnCreateConnection(from,to);

            // 输入约束：只能从 AbilityNode 或 Decorator 连接到本节点的 AbilityPort
            if(to.node == this && to.fieldName == nameof(AbilityPort)) {
                var parent = from.node;
                bool ok = parent is AbilityNode || parent is AbilityTriggerDecorateUnitNode;
                if(!ok) {
                    Debug.LogError("Trigger Decorator 只能出现在 Ability.TriggerPort 延伸的树上（父必须是 Ability 或 Trigger Decorator）。");
                    to.Disconnect(from);
                }
            }

            // 输出约束：Children 仅能连接到 Decorator 或 Trigger
            if(from.node == this && from.fieldName == nameof(Children)) {
                var child = to.node;
                bool ok = child is AbilityTriggerDecorateUnitNode || child is AbilityTriggerUnitNode;
                if(!ok) {
                    Debug.LogError("Trigger Decorator 的子节点必须是 Trigger 或 Trigger Decorator。");
                    from.Disconnect(to);
                }
            }
        }
    }
}