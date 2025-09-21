using System.Collections.Generic;
using UnityEngine;

namespace AbilitySystem {
    /// <summary>
    /// 技能行为逻辑编写的最小单位
    /// </summary>
    public abstract class AbilityBehaviorUnit : ScriptableObject {
        [field: SerializeField] public HeadInfo HeadInfo { get; set; }
        public int RuntimeToken { get; private set; }
        public bool unitExcutionFinished { get; private set; } = false;
        public bool unitExitFinished { get; private set; } = false;
        public abstract void OnTriggered(AbilityRuntimeContext abilityRuntimeContext);
        public abstract TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext);
        public abstract TaskStatus OnExit(AbilityRuntimeContext abilityRuntimeContext,bool allEffectFinished);
        public abstract TaskStatus OnInterrupt(InteruptionContext interuptionContext);

        [HideInInspector] public List<AbilityBehaviorUnit> Childs=new();
        public AbilityBehaviorUnit Child => Childs!= null && Childs.Count > 0 ? Childs[0] : null;

        public abstract AbilityBehaviorUnit Clone();
        
        public void OnBuild(List<AbilityBehaviorUnit> childs,int runtimeToken) { 
            Childs = childs;
            RuntimeToken = runtimeToken;
        }
    }
}