using NUnit.Framework;
using UnityEngine;

/// <summary>
/// 技能释放器，本质是一个顺序状态机，对技能控制器暴露技能释放接口
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
                if(currentStageIndex == skillConfig.skillExcutionStages.Count - 1) { // 技能释放完成
                    excuting = false;
                    return TaskStatus.Success;
                } else { // 进入下一个阶段
                    currentStageIndex++;
                    currentStage = skillConfig.skillExcutionStages[currentStageIndex];
                    return TaskStatus.ToBeContinue;
                }
            case TaskStatus.Failed:
                return TaskStatus.Failed;
        }

        Debug.LogError("未知状态");
        return TaskStatus.Failed;
    }
}
