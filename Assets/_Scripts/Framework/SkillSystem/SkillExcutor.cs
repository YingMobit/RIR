using NUnit.Framework;
using UnityEngine;

/// <summary>
/// �����ͷ�����������һ��˳��״̬�����Լ��ܿ�������¶�����ͷŽӿ�
/// </summary>
public class SkillExcutor : MonoBehaviour {
    [SerializeField] SkillConfig skillConfig;

    #region Runtime
    public SkillController Owner { get; private set; }
    public SkillExcutorScheduler Schduler { get; private set; }

    SkillExcutionStage currentStage;
    int currentStageIndex = 0;
    bool excuting = false;
    #endregion 

    public void OnLoad(SkillController skillController,SkillExcutorScheduler schduler) { 
        Owner = skillController;
        Schduler = schduler;
    }

    public TaskStatus Excute() {
        if(!excuting) {
            excuting = true;
            currentStageIndex = 0;
            currentStage = skillConfig.skillExcutionStages[0];
        }

        var res = currentStage.Run();
        switch(res) {
            case TaskStatus.ToBeContinue:
                return TaskStatus.ToBeContinue;
            case TaskStatus.Success:
                if(currentStageIndex == skillConfig.skillExcutionStages.Count - 1) { // �����ͷ����
                    excuting = false;
                    return TaskStatus.Success;
                } else { // ������һ���׶�
                    currentStageIndex++;
                    currentStage = skillConfig.skillExcutionStages[currentStageIndex];
                    return TaskStatus.ToBeContinue;
                }
            case TaskStatus.Failed:
                return TaskStatus.Failed;
        }

        Debug.LogError("δ֪״̬");
        return TaskStatus.Failed;
    }
}
