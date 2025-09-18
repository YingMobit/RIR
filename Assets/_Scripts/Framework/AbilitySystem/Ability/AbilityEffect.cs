using UnityEngine;

namespace AbilitySystem {
    /// <summary>
    /// 一个AbilityBehaviorUnit组，对一个游戏效果的打包
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