using PoolingSystem.ReferencePool;
using System.Collections.Generic;
namespace GAS {
    /// <summary>
    /// Ability Component依赖的运行时上下文。保存着技能系统配置，执行器，全局黑板等组件引用
    /// </summary>
    public class AbilityComponentContext : IReference<AbilityComponentContext> {
        public IReadOnlyDictionary<int,Ability> Abilities { get; private set; }
        public Dictionary<ControllerTypeEnum,IController> Controllers { get; private set; }
        public BlackBoard GlobalBlacboard { get; private set; }
        public AttributeSet AttributeSet { get; private set; }
        
        public void LoadAbilityConfig(Dictionary<int,Ability> abilities) { 
            Abilities = abilities;
            GlobalBlacboard = ReferencePoolingCenter.Instance.GetReference<BlackBoard>();
        }

        public void Bind(AttributeSet attributeSet) { 
            AttributeSet = attributeSet;
        }

        public void RegisterController(ControllerTypeEnum controllerType,IController controller) { 
            if(Controllers == null) { 
                Controllers = new Dictionary<ControllerTypeEnum,IController>();
            }
            Controllers[controllerType] = controller;
        }

        #region IReference
        public uint ReferenceType => ReferenceTypes.ABILITYCOMPONENTCONTEXT;

        int IReference.IndexInRefrencePool { get ; set ; }

        public void Dispose() {
            OnRecycle();
        }

        public IReference GetNewInstance() {
            return new AbilityComponentContext();
        }

        public void OnRecycle() {
            ReferencePoolingCenter.Instance.ReleaseReference(GlobalBlacboard);
            GlobalBlacboard = null;
            AttributeSet = null;
            Controllers.Clear();
            Controllers = null;
            Abilities = null;
        }
        #endregion
    }
}