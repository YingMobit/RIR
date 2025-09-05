using UnityEngine;

/// <summary>
/// 技能执行任务，定义技能逻辑的最小单位
/// </summary>
public abstract class SkillExcutionTask : ScriptableObject {
    public bool Crucial; // 关键任务，失败会导致整个阶段失败
    public SkillExcutionStage BelongsTo { get; private set; }
    public abstract TaskStatus OnLoad();
    public abstract TaskStatus Prepare();
    public abstract TaskStatus Run();
    public abstract void Exit();
}