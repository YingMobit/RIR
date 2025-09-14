using System.Collections.Generic;


namespace AbilitySystem {
    /// <summary>
    /// Ability运行时需要的所有信息，包括AbilityComponentContext和各Unit运行时数据缓存
    /// </summary>
    internal class AbilityRuntimeContext {
        public AbilityComponentContext AbilityComponentContext;
        public Dictionary<int,BlackBoard> LocalBlackBoards;
    
        public void Init(AbilityComponentContext abilityComponentContext){
            AbilityComponentContext = abilityComponentContext;
        }
    }
}