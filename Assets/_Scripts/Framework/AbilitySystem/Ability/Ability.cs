using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem {
    /// <summary>
    /// һ��Ability�������ļ�������ΪAbilityEffect���䴥����������
    /// </summary>
    public class Ability {
        [field: SerializeField] public HeadInfo AbilityHeadInfo { get; private set; }
        [field: SerializeField] public AbilityTriggerUnit TriggerUnit { get; private set; }
        [field: SerializeField] public List<AbilityEffect> Effects{ get; private set; }

        public void OnBuild(HeadInfo headInfo,AbilityTriggerUnit abilityTriggerUnit,List<AbilityEffect> effects) {
            AbilityHeadInfo = headInfo;
            TriggerUnit = abilityTriggerUnit;
            Effects = effects;
        }
    }
}