using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem {
    /// <summary>
    /// 技能行为逻辑编写的最小单位
    /// </summary>
    public abstract class AbilityBehaviorUnit : ScriptableObject {
        public HeadInfo HeadInfo { get; set; }
        public bool unitExcutionFinished { get; private set; } = false;
        public bool unitExitFinished { get; private set; } = false;
        public abstract void OnTriggered(AbilityRuntimeContext abilityRuntimeContext);
        public abstract TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext);
        public abstract TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext,bool allEffectFinished);
        public abstract TaskStatus OnInterrupt(IIntreruptionContext intreruptionContext);

        public List<AbilityBehaviorUnit> Childs=new();
        public AbilityBehaviorUnit Child => Childs!= null && Childs.Count > 0 ? Childs[0] : null;

        public abstract AbilityBehaviorUnit Clone();
        
        public void OnBuild(List<AbilityBehaviorUnit> childs) { 
            Childs = childs;
        }
    }
}