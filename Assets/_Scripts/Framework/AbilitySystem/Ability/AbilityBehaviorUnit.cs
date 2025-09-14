namespace AbilitySystem {
    /// <summary>
    /// ������Ϊ�߼���д����С��λ
    /// </summary>
    internal abstract class AbilityBehaviorUnit {
        public HeadInfo HeadInfo { get; set; }
        public abstract TaskStatus OnLoad();
        public abstract TaskStatus OnExcute(AbilityRuntimeContext abilityRuntimeContext);
        public abstract TaskStatus OnExit();
        public abstract TaskStatus OnInterrupt(IIntreruptionContext intreruptionContext);
    }
}