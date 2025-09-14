using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem {
    /// <summary>
    /// 一个Ability的配置文件，仅作为AbilityEffect及其触发器的容器
    /// </summary>
    internal class Ability {
        [field: SerializeField] public HeadInfo HeadInfo { get; }
        [field: SerializeField] public AbilityTriggerUnit TriggerUnit { get; }
        [field: SerializeField] public List<AbilityEffect> Effects{ get; }
    }
}