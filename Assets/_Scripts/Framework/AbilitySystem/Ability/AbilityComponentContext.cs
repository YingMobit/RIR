using System.Collections.Generic;

namespace AbilitySystem {
    /// <summary>
    /// Ability Component����������ʱ�����ġ������ż���ϵͳ���ã�ִ������ȫ�ֺڰ���������
    /// </summary>
    public class AbilityComponentContext {
        public IReadOnlyDictionary<int,Ability> Abilitys { get; private set; }
        public BlackBoard GlobalBlacboard { get; private set; }
        public Dictionary<int,IController> Controllers { get; private set; }
    }
}