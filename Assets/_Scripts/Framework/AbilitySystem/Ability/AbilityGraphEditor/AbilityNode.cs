using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace GAS.Editor.AbilityEditor {
    // A：Ability 的根节点（通常唯一）
    [CreateNodeMenu("Ability/Ability Root")]
    [DisallowMultipleNodes] // 限制图内仅一个
    public class AbilityNode : Node {
        [Header("Ability")]
        public HeadInfo AbilityHeadInfo;

        // 输出到多个 Effect
        [Output(backingValue = ShowBackingValue.Never,
            connectionType = ConnectionType.Multiple,
            typeConstraint = TypeConstraint.Strict,
            dynamicPortList = false)]
        public AbilityNode EffectsPorts;

        [Output(backingValue = ShowBackingValue.Never,
            connectionType = ConnectionType.Override,
            typeConstraint = TypeConstraint.Strict)]
        public AbilityTriggerUnitNode TriggerPort;

        public override object GetValue(NodePort port) => this;

        public override void OnCreateConnection(NodePort from,NodePort to) {
            base.OnCreateConnection(from,to);

            // 效果排序
            if(from.node.GetType() == typeof(AbilityNode) && to.node.GetType() == typeof(AbilityEffectNode)) {
                int index = 0;
                foreach(var port in from.GetConnections()) {
                    (port.node as AbilityEffectNode).SetOrder(this,index++);
                }
                return;
            }

            // 触发口约束：只能连接 Trigger 或 Trigger Decorator，且仅一个
            if(from.node == this && from.fieldName == nameof(TriggerPort)) {
                var ok = to.node is AbilityTriggerUnitNode || to.node is AbilityTriggerDecorateUnitNode;
                if(!ok) {
                    Debug.LogError("Ability.TriggerPort 只能连接 Trigger 或 Trigger Decorator。");
                    from.Disconnect(to);
                }
            }
        }

        public override void OnRemoveConnection(NodePort port) {
            base.OnRemoveConnection(port);
            if(port.node.GetType() != typeof(AbilityNode))
                return;
            int index = 0;
            foreach(var connection in port.GetConnections()) {
                (connection.node as AbilityEffectNode).SetOrder(this,index++);
            }
        }

        public IEnumerable<AbilityEffectNode> GetEffects() {
            var port = GetOutputPort(nameof(EffectsPorts));
            if(port == null)
                yield break;
            foreach(var conn in port.GetConnections()) {
                if(conn?.node is AbilityEffectNode e)
                    yield return e;
            }
        }

        public AbilityEffectNode GetEffect(int order) {
            int index = 0;
            foreach(var effect in GetEffects()) {
                if(index++ == order)
                    return effect;
            }
            return null;
        }

        public Ability Build() {
            var ability = new Ability();
            AbilityTriggerUnit triggerUnit = null;
            List<AbilityEffect> effects = new List<AbilityEffect>();
            foreach(var port in Ports) {
                if(port.fieldName == nameof(TriggerPort)) {
                    var node = port.GetConnection(0)?.node;
                    if(node is AbilityTriggerUnitNode tNode) {
                        triggerUnit = tNode.Build();
                    } else if(node is AbilityTriggerDecorateUnitNode dNode) {
                        // 构建时校验：Decorator 不可为叶子，递归构建
                        triggerUnit = dNode.Build();
                    } else if(node != null) {
                        Debug.LogError("Ability.TriggerPort 必须连接到 Trigger 或 Trigger Decorator。");
                    }
                } else {
                    foreach(var conn in port.GetConnections()) {
                        if(conn?.node is AbilityEffectNode e) {
                            effects.Add(e.Build());
                        }
                    }
                }
            }
            ability.OnBuild(AbilityHeadInfo,triggerUnit,effects);
            return ability;
        }
    }
}