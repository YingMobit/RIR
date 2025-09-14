using System;
using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem {
    /// <summary>
    /// һ��Ability�������ļ�������ΪAbilityEffect���䴥����������
    /// </summary>
    internal class Ability {
        [field: SerializeField] public HeadInfo HeadInfo { get; }
        [field: SerializeField] public AbilityTriggerUnit TriggerUnit { get; }
        [field: SerializeField] public List<AbilityEffect> Effects{ get; }
    }
}