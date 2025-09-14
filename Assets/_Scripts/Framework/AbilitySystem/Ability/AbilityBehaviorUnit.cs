namespace AbilitySystem {
    /// <summary>
    /// 技能行为逻辑编写的最小单位
    /// </summary>
    internal abstract class AbilityBehaviorUnit {
        public HeadInfo HeadInfo { get; set; }
        public abstract TaskStatus OnLoad();
        public abstract TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext);
        public abstract TaskStatus OnExit();
        public abstract TaskStatus OnInterrupt(IIntreruptionContext intreruptionContext);
    }
}