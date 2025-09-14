using System.Collections.Generic;


namespace AbilitySystem {
    /// <summary>
    /// Ability����ʱ��Ҫ��������Ϣ������AbilityComponentContext�͸�Unit����ʱ���ݻ���
    /// </summary>
    internal class AbilityRuntimeContext {
        public AbilityComponentContext AbilityComponentContext;
        public Dictionary<int,BlackBoard> LocalBlackBoards;
    
        public void Init(AbilityComponentContext abilityComponentContext){
            AbilityComponentContext = abilityComponentContext;
        }
    }
}