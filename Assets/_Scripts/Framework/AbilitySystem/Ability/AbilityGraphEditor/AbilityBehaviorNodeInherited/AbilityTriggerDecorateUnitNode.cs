using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace GAS.Editor.AbilityEditor {
    [CreateNodeMenu("Ability/Trigger Decorator")]
    [NodeTint("#6b8e23")]
    public class AbilityTriggerDecorateUnitNode : Node {
        // ����AbilityNode.TriggerPort �� ��һ�� Decorator
        [Input(backingValue = ShowBackingValue.Never,
               connectionType = ConnectionType.Override,
               typeConstraint = TypeConstraint.Strict)]
        public AbilityTriggerUnitNode AbilityPort;

        // �ӣ���� Decorator �� �ս� Trigger
        [Output(backingValue = ShowBackingValue.Never,
                connectionType = ConnectionType.Multiple,
                typeConstraint = TypeConstraint.Strict)]
        public AbilityTriggerUnitNode Children;

        // װ������Դ�������ʵ���и�������Ӵ������߼���
        [field: SerializeField] public AbilityTriggerDecorateUnit abilityTriggerDecorator { get; private set; }

        public override object GetValue(NodePort port) => this;

        // �������ռ������� Trigger/Decorator��װ��������ΪҶ��
        public AbilityTriggerUnit Build() {
            var outPort = GetOutputPort(nameof(Children));
            if(outPort == null) {
                Debug.LogError("Trigger Decorator ȱ�� Children ����˿ڡ�");
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
                    Debug.LogError("Trigger Decorator ���ӽڵ������ Trigger �� Trigger Decorator��");
                    return null;
                }
            }

            if(childUnits.Count == 0) {
                Debug.LogError("Trigger Decorator ������ΪҶ�ӽڵ㣺������������һ���� Trigger/Decorator��");
                return null;
            }

            (res as AbilityTriggerDecorateUnit).OnBuild(childUnits);
            return res;
            
        }

        public override void OnCreateConnection(NodePort from,NodePort to) {
            base.OnCreateConnection(from,to);

            // ����Լ����ֻ�ܴ� AbilityNode �� Decorator ���ӵ����ڵ�� AbilityPort
            if(to.node == this && to.fieldName == nameof(AbilityPort)) {
                var parent = from.node;
                bool ok = parent is AbilityNode || parent is AbilityTriggerDecorateUnitNode;
                if(!ok) {
                    Debug.LogError("Trigger Decorator ֻ�ܳ����� Ability.TriggerPort ��������ϣ��������� Ability �� Trigger Decorator����");
                    to.Disconnect(from);
                }
            }

            // ���Լ����Children �������ӵ� Decorator �� Trigger
            if(from.node == this && from.fieldName == nameof(Children)) {
                var child = to.node;
                bool ok = child is AbilityTriggerDecorateUnitNode || child is AbilityTriggerUnitNode;
                if(!ok) {
                    Debug.LogError("Trigger Decorator ���ӽڵ������ Trigger �� Trigger Decorator��");
                    from.Disconnect(to);
                }
            }
        }
    }
}