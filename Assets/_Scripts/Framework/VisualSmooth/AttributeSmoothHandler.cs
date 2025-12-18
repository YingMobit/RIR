using System;
using System.Collections.Generic;
using UnityEngine;

public class AttributeSmoothHandler<TValue> where TValue : struct, IEquatable<TValue> {
    #region Smooth
    private Dictionary<int,AttributeSmoothTask<TValue>> _taskMap = new Dictionary<int,AttributeSmoothTask<TValue>>();

    private List<int> _pendingRemoveList = new List<int>();

    /// <summary>
    /// 注册平滑任务
    /// </summary>
    /// <param name="taskID">任务ID</param>
    /// <param name="logicValue">ֵ逻辑值</param>
    /// <param name="smoothFrames">目标平滑帧数</param>
    /// <param name="applyVisualValue">应用平滑插值的回调</param>
    /// <param name="lerpMethod">平滑方法</param>
    public void RegisterTask(
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