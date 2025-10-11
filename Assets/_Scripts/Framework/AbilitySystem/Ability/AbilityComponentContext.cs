using System.Collections.Generic;
namespace GAS {
    /// <summary>
    /// Ability Component依赖的运行时上下文。保存着技能系统配置，执行器，全局黑板等组件引用
    /// </summary>
    public class AbilityComponentContext {
        public IReadOnlyDictionary<int,Ability> Abilities { get; private set; }
        public BlackBoard GlobalBlacboard { get; private set; }
        public Dictionary<ControllerTypeEnum,IController> Controllers { get; private set; }
        public AttributeSet AttributeSet { get; private set; }
        public AbilityComponentContext(Dictionary<int,Ability> abilities,BlackBoard blackBoard,Dictionary<ControllerTypeEnum,IController> controllers,AttributeSet attributeSet) { 
            Abilities = abilities;
            GlobalBlacboard = blackBoard;
            Controllers = controllers;
            AttributeSet = attributeSet;
        }
    }
}