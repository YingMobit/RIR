using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace AbilitySystem {
    /// <summary>
    /// 用于管理配置好的Ability的运行时状态
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
            //更新运行中的Task的上下文
            //运行Task
            //根据运行结果重整状态
        }

        public void LateUpdate(){ 
            
        }
        #endregion
    }
}