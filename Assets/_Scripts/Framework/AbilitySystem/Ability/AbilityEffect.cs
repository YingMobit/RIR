using UnityEngine;

namespace AbilitySystem {
    /// <summary>
    /// һ��AbilityBehaviorUnit�飬��һ����ϷЧ���Ĵ��
    /// </summary>
    public class AbilityEffect {
        public HeadInfo EffectHeadInfo { get; private set; }
        public AbilityBehaviorUnit RootBehaviorUnit { get; private set; }

        public void OnBuild(HeadInfo headInfo,AbilityBehaviorUnit root) {
            EffectHeadInfo = headInfo;
            RootBehaviorUnit = root;
        }
    }
}