using System.Collections.Generic;
namespace GAS {
    /// <summary>
    /// Ability Component����������ʱ�����ġ������ż���ϵͳ���ã�ִ������ȫ�ֺڰ���������
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