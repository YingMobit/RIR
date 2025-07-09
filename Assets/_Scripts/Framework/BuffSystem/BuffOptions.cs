public enum BuffStackUpStrategy {
    //重置运行时间
    ResetRunTime,
    //提高层数但是保持运行时间不变
    AddStackOnly,
    //延长Buff持续时间
    AddDuration,
    //提高层数并重置运行时间
    AddStackAndResetRunTime,
    //并行
    Independent
}

public enum BuffStackDownStrategy {
    Reduce,
    Clear,
    //并行
    Independent
}

public enum BuffTag {
    //正面buff
    Positive,
    //负面buff
    Negative,
    //中性buff
    Neuter
}

public enum BuffTriggerType {
    //周期性的
    Tick,
    //事件型buff
    EventTrigger
}

namespace BuffSystem.Config {
    public static class BuffSystemConfig {
        public const string BuffConfigDataMenuPath = "ScriptableObject/BuffData/BuffConfigData";
        public const string BuffEffectDataMenuPathRoot = "ScriptableObject/BuffData/BuffEffects/";
    }
}