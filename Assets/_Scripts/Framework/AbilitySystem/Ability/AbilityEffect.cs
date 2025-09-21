using UnityEngine;

namespace AbilitySystem {
    /// <summary>
    /// һ��AbilityBehaviorUnit�飬��һ����ϷЧ���Ĵ��
    /// </summary>
    public class AbilityEffect {
        public HeadInfo EffectHeadInfo { get; private set; }
        public int InteruptionPriority{ get; private set; }
        public AbilityBehaviorUnit RootBehaviorUnit { get; private set; }

        public void OnBuild(HeadInfo headInfo,int interuptionPriority,AbilityBehaviorUnit root) {
            EffectHeadInfo = headInfo;
            InteruptionPriority = interuptionPriority;
            RootBehaviorUnit = root;
        }
    }
}