using UnityEngine;

/// <summary>
/// ����ִ�����񣬶��弼���߼�����С��λ
/// </summary>
public abstract class SkillExcutionTask : ScriptableObject {
    public bool Crucial; // �ؼ�����ʧ�ܻᵼ�������׶�ʧ��
    public SkillExcutionStage BelongsTo { get; private set; }
    public abstract TaskStatus OnLoad();
    public abstract TaskStatus Prepare();
    public abstract TaskStatus Run();
    public abstract void Exit();
}