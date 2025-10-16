using System;
using System.Collections.Generic;
using UnityEngine;

namespace GAS {
    /// <summary>
    /// 一个Ability的配置文件，仅作为AbilityEffect及其触发器的容器
    /// </summary>
    public class Ability {
        [field: SerializeField] public HeadInfo AbilityHeadInfo { get; private set; }
        [field: SerializeField] public AbilityTriggerUnit TriggerUnit { get; private set; }
        [field: SerializeField] public List<AbilityEffect> Effects{ get; private set; }
        [field: SerializeField] public bool Stackable { get; private set; } = false;

        public void OnBuild(HeadInfo headInfo,AbilityTriggerUnit abilityTriggerUnit,List<AbilityEffect> effects,bool stackable) {
            AbilityHeadInfo = headInfo;
            TriggerUnit = abilityTriggerUnit;
            Effects = effects;
            Stackable = stackable;
        }
    }
}