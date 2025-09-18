using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace AbilitySystem.Editor.AbilityEditor {
    [CreateNodeMenu("Ability/BehaviorUnit/Decorator")]
    public class AbilityDecorateUnitNode : AbilityBehaviorUnitNode {
        [Input(backingValue = ShowBackingValue.Never,
            connectionType = ConnectionType.Override,
            typeConstraint = TypeConstraint.Inherited)]
        public AbilityBehaviorUnitNode compositeNode;

        [Output(backingValue = ShowBackingValue.Never,
            connectionType = ConnectionType.Override,
            typeConstraint = TypeConstraint.Inherited)]
        public AbilityBehaviorUnitNode actionNode;

        [field: SerializeField] public AbilityDecorateUnit AbilityDecoratorUnit { get; private set; }

        // Use this for initialization
        protected override void Init() {
    		base.Init();
    	}
    
    	// Return the correct value of an output port when requested
    	public override object GetValue(NodePort port) {
    		return null; // Replace this
    	}

        public override AbilityBehaviorUnit Build() {
            AbilityBehaviorUnit unit = AbilityDecoratorUnit.Clone();
            List<AbilityBehaviorUnit> childs = new List<AbilityBehaviorUnit>();
            NodePort outPort = GetOutputPort(nameof(actionNode));
            foreach(var conn in outPort.GetConnections()) { 
                childs.Add((conn.node as AbilityBehaviorUnitNode).Build());
            }
            unit.OnBuild(childs);
            return unit;
        }
    }
}
