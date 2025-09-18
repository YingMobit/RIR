using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace AbilitySystem.Editor.AbilityEditor {
    [CreateNodeMenu("Ability/Trigger")]
    [NodeTint("#5c7080")]
    public class AbilityTriggerUnitNode : Node {
        [Input(backingValue = ShowBackingValue.Never,
            connectionType = ConnectionType.Override,
            typeConstraint = TypeConstraint.Strict)]
        public AbilityTriggerUnitNode AbilityPort;

        [field: SerializeField] public AbilityTriggerUnit TriggerUnit { get; private set; }

        public AbilityTriggerUnit Build() {
            return TriggerUnit;
        }
    }
}