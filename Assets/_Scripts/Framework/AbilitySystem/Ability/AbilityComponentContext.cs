using System.Collections.Generic;
namespace GAS {
    /// <summary>
    /// Ability Component����������ʱ�����ġ������ż���ϵͳ���ã�ִ������ȫ�ֺڰ���������
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