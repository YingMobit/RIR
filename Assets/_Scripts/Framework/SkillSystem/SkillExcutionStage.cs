using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 技能执行阶段，同于管理一个技能在某个阶段的所有顺序任务
/// </summary>
public class SkillExcutionStage : ScriptableObject {
    public List<SkillExcutionTask> skillExcutionTasks;

    #region Runtime
    public SkillExcutor Excutor { get; private set; }

    SkillExcutionTask currentTask;
    int currentTaskIndex = 0;
    bool stageRunning = false;
    bool currentTaskPreparing = true;
    Func<TaskStatus> nextFrame;
    #endregion

    public void Init() { 
        
    }

    public TaskStatus Run() {
        if(!stageRunning) { // 开始新一轮执行
            if(skillExcutionTasks.Count == 0) return TaskStatus.Success; 
            stageRunning = true;
            currentTaskIndex = 0;
            currentTask = skillExcutionTasks[0];
            currentTaskPreparing = true;
            var res = currentTask.Prepare();
            switch(res) {
                case TaskStatus.ToBeContinue:
                    nextFrame = currentTask.Prepare;
                    return TaskStatus.ToBeContinue;
                case TaskStatus.Success:
                    currentTaskPreparing = false;
                    nextFrame = currentTask.Run;
                    return TaskStatus.ToBeContinue;
                case TaskStatus.Failed:
                    if(currentTask.Crucial) {
                        Exit();
                        return TaskStatus.Failed;
                    } else {
                        if(skillExcutionTasks.Count == 1) { //比较特殊的情况，只有一个非关键任务，并且失败了
                            Exit();
                            Debug.LogAssertion($"此阶段只有一个非关键任务，但是他执行失败了: {currentTask.name}");
                            return TaskStatus.Success;
                        } else { // 进入下一个任务
                            return TurnToNextTask();
                        }
                    }
            }
        } else { 
            var res = nextFrame?.Invoke();
            if(currentTaskPreparing) { // 当前任务未就绪
                switch(res) {
                    case TaskStatus.ToBeContinue: // 继续等待
                        return TaskStatus.ToBeContinue;
                    case TaskStatus.Success: // 准备就绪，下一帧开始执行
                        currentTaskPreparing = false;
                        nextFrame = currentTask.Run;
                        return TaskStatus.ToBeContinue;
                    case TaskStatus.Failed: // 准备失败，整个阶段失败
                        Exit();
                        return TaskStatus.Failed;
                }
            } else {
                switch(res) {
                    case TaskStatus.ToBeContinue: // 继续等待
                        return TaskStatus.ToBeContinue;
                    case TaskStatus.Success: // 当前任务执行完毕，进入下一个任务
                        return TurnToNextTask();
                    case TaskStatus.Failed: // 当前任务执行失败
                        if(currentTask.Crucial) {
                            Exit();
                            return TaskStatus.Failed;
                        } else {
                            return TurnToNextTask();
                        }                            
                }
            }
        }

        Debug.LogError("未知状态");
        return TaskStatus.Failed;
    }

    public void Exit() {
        for(int i=0;i <= currentTaskIndex ;i++ ) { 
            skillExcutionTasks[i].Exit();
        }
        currentTask = null;
        currentTaskIndex = 0;
        stageRunning = false;
        currentTaskPreparing = true;
        nextFrame = null;   
    }

    private TaskStatus TurnToNextTask() {
        if(currentTaskIndex == skillExcutionTasks.Count - 1) { // 最后一个任务执行完毕，整个阶段成功
            Exit();
            return TaskStatus.Success;
        } else { // 进入下一个任务
            currentTaskIndex++;
            currentTask = skillExcutionTasks[currentTaskIndex];
            currentTaskPreparing = true;
            nextFrame = currentTask.Prepare;
            return TaskStatus.ToBeContinue;
        }
    }
}