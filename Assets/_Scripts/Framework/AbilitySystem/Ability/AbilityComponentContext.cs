using System.Collections.Generic;
namespace GAS {
    /// <summary>
    /// Ability Component依赖的运行时上下文。保存着技能系统配置，执行器，全局黑板等组件引用
    /// </summary>
    public class AbilityComponentContext {
        public IReadOnlyDictionary<int,Ability> Abilities { get; private set; }
        public BlackBoard GlobalBlacboard { get; private set; }
        public Dictionary<int,IController> Controllers { get; private set; }
        public Dictionary<int,int> AbilityIDTokenMap { get; private set; }

        public AbilityComponentContext(Dictionary<int,Ability> abilities,BlackBoard blackBoard,Dictionary<int,IController> controllers) { 
            Abilities = abilities;
            GlobalBlacboard = blackBoard;
            Controllers = controllers;
        }
    }
}