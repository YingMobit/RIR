using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace GAS.Editor.AbilityEditor {
    [CreateNodeMenu("Ability/Behavior/Base (abstract)")]
    [NodeTint("#5c7080")]
    public abstract class AbilityBehaviorUnitNode : Node {
        public abstract HeadInfo HeadInfo { get; }
        [field: SerializeField, ReadOnly] public int Order { get; private set; }
        [field: SerializeField,ReadOnly] public int RuntimeToken { get; set; }

        [SerializeField] protected List<UnitNodeRefrenceRecord> unitNodeRefrences = new();
        protected abstract AbilityBehaviorUnit AbilityBehaviorUnit { get; }
        public void SetOrder(AbilityBehaviorUnitNode node,int order) {
            Order = order;
        }

        public override void OnCreateConnection(NodePort from,NodePort to) {
            base.OnCreateConnection(from,to);
            if(from.node == this) {
                int index = 0;
                int nextToken = RuntimeToken+1;
                foreach(var port in from.GetConnections()) {
                    (port.node as AbilityBehaviorUnitNode).SetOrder(this,index++);
                    if(RuntimeToken != -1) {
                        nextToken = (port.node as AbilityBehaviorUnitNode).SetRuntimeToken(nextToken);
                        OnTokenDisplayed(nextToken-1);
                    }
                }
            } else {
                if(from.node.GetType() == typeof(AbilityEffectNode))
                    Order = 0;
            }
        }

        public abstract void OnTokenDisplayed(int newToken);

        public override void OnRemoveConnection(NodePort port) {
            base.OnRemoveConnection(port);
            if(port.IsOutput) {
                int index = 0;
                foreach(var connection in port.GetConnections()) {
                    (connection.node as AbilityBehaviorUnitNode).SetOrder(this,index++);
                }
            } else {
                Order = -1;
                RuntimeToken = -1;
            }
        }

        [PropertySpace(10)]
        [Button("RefreshUnitRefrenceList")]
        protected void RefreshUnitRefrenceList() {
            if(AbilityBehaviorUnit != null) {
                unitNodeRefrences.Clear();
                var fields = AbilityBehaviorUnit.GetType().GetFields();
                foreach(var field in fields) {
                    if(field.FieldType == typeof(UnitNodeRefrence)) {
                        unitNodeRefrences.Add(new(field.Name,new(-1)));
                    }
                }
            } else {
                unitNodeRefrences.Clear();
            }
        }

        protected void InjectUnitNodeRefrence(AbilityBehaviorUnit abilityBehaviorUnit) {
            var fields = abilityBehaviorUnit.GetType().GetFields();
            foreach(var field in fields) {
                if(field.FieldType == typeof(UnitNodeRefrence)) {
                    field.SetValue(abilityBehaviorUnit,unitNodeRefrences.Find(x=>x.FieldName == field.Name).RunTimeToken);
                }
            }
        }

        public abstract AbilityBehaviorUnit Build();

        public abstract int SetRuntimeToken(int token);
    }
}
