using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace AbilitySystem {
    /// <summary>
    /// ���ڹ������úõ�Ability������ʱ״̬
    /// </summary>
    internal class AbilityComponent {
        Dictionary<string,Ability> activeAbility;
        Dictionary<string,AbilityExcutionTask> runningTasks;
        
        #region API
        public void RegistAbility(string abilityName) { 
            
        }

        public void InterruptAbility(IIntreruptionContext intreruptionContext) { 
            
        }

        public void RemoveAbility(string abilityName) { 
            
        }
        #endregion

        #region Life Time
        public void Update(AbilityComponentContext abilityComponentContext) { 
            //���������е�Task��������
            //����Task
            //�������н������״̬
        }

        public void LateUpdate(){ 
            
        }
        #endregion
    }
}