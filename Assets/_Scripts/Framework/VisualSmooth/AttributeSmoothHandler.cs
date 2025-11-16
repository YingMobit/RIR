using System;
using System.Collections.Generic;
using UnityEngine;

public class AttributeSmoothHandler<TValue> where TValue : struct, IEquatable<TValue> {
    #region Smooth
    private Dictionary<int,AttributeSmoothTask<TValue>> _taskMap = new Dictionary<int,AttributeSmoothTask<TValue>>();

    private List<int> _pendingRemoveList = new List<int>();

    /// <summary>
    /// 注册或更新平滑任务
    /// </summary>
    /// <param name="taskID">任务ID</param>
    /// <param name="logicValue">目标逻辑值</param>
    /// <param name="smoothFrames">期望多少帧完成平滑</param>
    /// <param name="applyVisualValue">应用视觉值的回调</param>
    /// <param name="lerpMethod">插值方法</param>
    public void RegistTask(
        int taskID,
        TValue visualValue,
        TValue logicValue,
        int smoothFrames,
        Action<TValue> applyVisualValue,
        Func<TValue,TValue,float,TValue> lerpMethod,
        Func<TValue,TValue,bool> equal) {
        if(_taskMap.TryGetValue(taskID,out var existingTask)) {
            existingTask.UpdateLogicValue(logicValue,smoothFrames);
        } else {
            // 创建新任务
            var newTask = new AttributeSmoothTask<TValue>();
            newTask.RegistTask(taskID,visualValue,logicValue,smoothFrames,applyVisualValue,lerpMethod,equal);
            _taskMap[taskID] = newTask;
        }
    }

    public void SyncVisualToLogic(int taskID) {
        if(_taskMap.TryGetValue(taskID,out var task)) {
            task.SyncVisualToLogic();
        }
    }

    public void Update(float deltaTime) {
        _pendingRemoveList.Clear();

        foreach(var kvp in _taskMap) {
            if(kvp.Value.Update(deltaTime)) {
                // 任务完成,标记为待移除
                _pendingRemoveList.Add(kvp.Key);
            }
        }

        foreach(var taskID in _pendingRemoveList) {
            _taskMap.Remove(taskID);
        }
    }

    public void Reset() {
        foreach(var task in _taskMap.Values) {
            task.Reset();
        }
        _taskMap.Clear();
        _pendingRemoveList.Clear();
    }
    #endregion

}