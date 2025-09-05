using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 技能配置文件，用于描述技能在每个阶段的行为
/// </summary>
public class SkillConfig : ScriptableObject {
    public List<SkillExcutionStage> skillExcutionStages;
}