using System.Collections.Generic;

namespace AbilitySystem {
    /// <summary>
    /// Ability Component依赖的运行时上下文。保存着技能系统配置，执行器，全局黑板等组件引用
    /// </summary>
    public class AbilityComponentContext {
        public IReadOnlyDictionary<int,Ability> Abilitys { get; private set; }
        public BlackBoard GlobalBlacboard { get; private set; }
        public Dictionary<int,IController> Controllers { get; private set; }
    }
}