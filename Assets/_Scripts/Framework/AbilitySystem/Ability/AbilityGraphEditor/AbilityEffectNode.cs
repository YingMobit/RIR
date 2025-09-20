using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace AbilitySystem.Editor.AbilityEditor {
    [CreateNodeMenu("Ability/Effect")]
    public class AbilityEffectNode : Node {
        [Header("Effect")]
        public HeadInfo EffectHeadInfo;
        [Tooltip("在 Ability.Effects 中的排序（编译/导出时使用）")]
        [field: SerializeField,ReadOnly] public int Order { get; private set; }
        public int startToken { get; private set; }
        public int maxRuntimeToken { get; private set; }

        // 父：Ability（单连覆盖）
        [Input(backingValue: ShowBackingValue.Never,
               connectionType: ConnectionType.Override,
               typeConstraint: TypeConstraint.Strict)]
        public AbilityNode Ability;

        // 子：该 Effect 的行为树根（可多根）
        [Output(backingValue = ShowBackingValue.Never,
            connectionType = ConnectionType.Override,
            typeConstraint = TypeConstraint.Inherited)] 
        public AbilityBehaviorUnitNode Behaviors;

        public override void OnCreateConnection(NodePort from,NodePort to) {
            base.OnCreateConnection(from,to);
            if(from.node == this) {
                maxRuntimeToken = (to.node as AbilityBehaviorUnitNode).SetRuntimeToken(startToken) - 1;
            } else {
                RefreshRuntimeToken();
            }
        }
        
        public override void OnRemoveConnection(NodePort port) {
            base.OnRemoveConnection(port);
            if(port.direction == NodePort.IO.Input)
                Order = -1;
        }

        public void RefreshRuntimeToken(){
            if(Order == 0) {
                startToken = 0;
            } else if(Order > 0) {
                startToken = (GetInputPort(nameof(Ability)).Connection.node as AbilityNode).GetEffect(Order - 1).maxRuntimeToken+1;
            } else {
                Debug.LogError("Order didn't set yet");
                return;
            }
            AbilityBehaviorUnitNode node = GetOutputPort(nameof(Behaviors)).Connection?.node as AbilityBehaviorUnitNode;
            if(node != null)
                maxRuntimeToken = node.SetRuntimeToken(startToken) - 1;
        }

        public void SetOrder(AbilityNode node,int order) {
            if(node == null)
                return;
            Order = order;
        }

        public void OnBehaviorNodeDisplayedToken(int newToken) {
            maxRuntimeToken = newToken;
            var abilityNode = (GetInputPort(nameof(Ability)).Connection.node as AbilityNode);
            foreach(var effect in abilityNode.GetEffects()) { 
                if(effect.Order > Order) {
                    effect.RefreshRuntimeToken();
                } 
            }
        }

        public AbilityEffect Build() { 
            AbilityEffect effect = new AbilityEffect();
            foreach(var port in GetOutputPort(nameof(Behaviors)).GetConnections()) {
                if(port.node is AbilityBehaviorUnitNode node) { 
                    effect.OnBuild(EffectHeadInfo,node.Build());
                }
            }
            return effect;
        }

        public override object GetValue(NodePort port) => null;
    }
}

