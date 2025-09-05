using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����ִ�н׶Σ�ͬ�ڹ���һ��������ĳ���׶ε�����˳������
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
        if(!stageRunning) { // ��ʼ��һ��ִ��
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
                        if(skillExcutionTasks.Count == 1) { //�Ƚ�����������ֻ��һ���ǹؼ����񣬲���ʧ����
                            Exit();
                            Debug.LogAssertion($"�˽׶�ֻ��һ���ǹؼ����񣬵�����ִ��ʧ����: {currentTask.name}");
                            return TaskStatus.Success;
                        } else { // ������һ������
                            return TurnToNextTask();
                        }
                    }
            }
        } else { 
            var res = nextFrame?.Invoke();
            if(currentTaskPreparing) { // ��ǰ����δ����
                switch(res) {
                    case TaskStatus.ToBeContinue: // �����ȴ�
                        return TaskStatus.ToBeContinue;
                    case TaskStatus.Success: // ׼����������һ֡��ʼִ��
                        currentTaskPreparing = false;
                        nextFrame = currentTask.Run;
                        return TaskStatus.ToBeContinue;
                    case TaskStatus.Failed: // ׼��ʧ�ܣ������׶�ʧ��
                        Exit();
                        return TaskStatus.Failed;
                }
            } else {
                switch(res) {
                    case TaskStatus.ToBeContinue: // �����ȴ�
                        return TaskStatus.ToBeContinue;
                    case TaskStatus.Success: // ��ǰ����ִ����ϣ�������һ������
                        return TurnToNextTask();
                    case TaskStatus.Failed: // ��ǰ����ִ��ʧ��
                        if(currentTask.Crucial) {
                            Exit();
                            return TaskStatus.Failed;
                        } else {
                            return TurnToNextTask();
                        }                            
                }
            }
        }

        Debug.LogError("δ֪״̬");
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
        if(currentTaskIndex == skillExcutionTasks.Count - 1) { // ���һ������ִ����ϣ������׶γɹ�
            Exit();
            return TaskStatus.Success;
        } else { // ������һ������
            currentTaskIndex++;
            currentTask = skillExcutionTasks[currentTaskIndex];
            currentTaskPreparing = true;
            nextFrame = currentTask.Prepare;
            return TaskStatus.ToBeContinue;
        }
    }
}