using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用于配置技能和输入的配置类，并对外暴露技能释放接口
/// </summary>
public class SkillController : MonoBehaviour {
    #region Struct Defination
    [Serializable]
    private struct SkillConfig {
        public SkillInput skillInput;
        public SkillExcutor skillExcutorPrefab;
        public int basicSkillChargeCount;
    }
    #endregion

    [SerializeField] List<SkillConfig> skillConfigs;

    #region Runtime;
    Dictionary<SkillInput,SkillExcutorScheduler> runtimeSkillInputExcutorSchdulerMap = new();
    Dictionary<SkillExcutor,SkillExcuteStatusHandler> activeSkillExcuteStatusHandlerMap = new();
    #endregion

    private void Awake() {
        LoadExcutors();
    }

    void LoadExcutors() { 
        foreach(var config in skillConfigs) {
            if(runtimeSkillInputExcutorSchdulerMap.ContainsKey(config.skillInput)) { 
                Debug.LogError($"SkillController {name} 中存在重复的SkillInput配置: {config.skillInput}");
                return;
            }

            GameObject skillExcutorRoot = new GameObject($"SkillExcutors_{config.skillInput}");
            var scheduler = skillExcutorRoot.AddComponent<SkillExcutorScheduler>();
            skillExcutorRoot.transform.SetParent(transform);
            var skillExcutorCount = config.basicSkillChargeCount!=0? config.basicSkillChargeCount : 1;  
            scheduler.OnLoad(this,config.skillExcutorPrefab,skillExcutorCount);
            runtimeSkillInputExcutorSchdulerMap.Add(config.skillInput,scheduler);
        }
    }

    public SkillExcuteStatusHandler Excute(SkillInput skillInput) {
        var excutor = runtimeSkillInputExcutorSchdulerMap[skillInput].Schedule();
        if(excutor == null) { 
            return null;
        }
        SkillExcuteStatusHandler handler = new SkillExcuteStatusHandler() { Vaild = true,Status = TaskStatus.ToBeContinue };

        if(activeSkillExcuteStatusHandlerMap.ContainsKey(excutor)) {
            activeSkillExcuteStatusHandlerMap[excutor].Vaild = false; // 标记之前的状态处理器无效
            activeSkillExcuteStatusHandlerMap[excutor] = handler;
        } else {
            activeSkillExcuteStatusHandlerMap.Add(excutor,handler);
        }

        return handler;
    }

    private void LateUpdate() {
        List<SkillExcutor> toRemove = new List<SkillExcutor>();
        foreach(var pair in activeSkillExcuteStatusHandlerMap) {
            pair.Value.Status = pair.Key.Excute();
            switch(pair.Value.Status) {
                case TaskStatus.Success:
                    toRemove.Add(pair.Key);
                    break;
                case TaskStatus.ToBeContinue:
                    break;
                case TaskStatus.Failed:
                    toRemove.Add(pair.Key);
                    break;
            }
        }

        foreach(var excutor in toRemove) { 
            activeSkillExcuteStatusHandlerMap.Remove(excutor);
            excutor.Schduler.Recycle(excutor);
        }
    }
}

public class SkillExcuteStatusHandler {
    public bool Vaild;//若为false，表示原SkillExcutor已被复用
    public TaskStatus Status;
}
