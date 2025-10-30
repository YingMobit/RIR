using System;
using System.Collections.Generic;
using UnityEngine.Pool;

public class ValueSmoothHandler<TValue> where TValue : struct , IEquatable<TValue> {
    Dictionary<int,ValueSmoothTask<TValue>> taskMap = new();

    public void RegistTask(int id,TValue initialValue,TValue targetValue,float smoothTime,Action<TValue> applyData,Func<TValue,TValue,float,TValue> lerpMethod) {
        ValueSmoothTask<TValue> task;
        if(taskMap.ContainsKey(id)) {
            task = taskMap[id];
        } else { 
            task = new ValueSmoothTask<TValue>();
        }

        task.RegistTask(initialValue,targetValue,smoothTime,applyData,lerpMethod);
        taskMap[id] = task;
    }

    public void Update() {
        List<ValueSmoothTask<TValue>> removeList = ListPool<ValueSmoothTask<TValue>>.Get();
        foreach(var task in taskMap.Values) {
            if(task.Update()) { 
                removeList.Add(task);
            }
        }
        foreach(var task in removeList) {
            taskMap.Remove(task.GetHashCode());
        }
        removeList.Clear();
        ListPool<ValueSmoothTask<TValue>>.Release(removeList);
    }

    public void Reset() { 
        taskMap.Clear();
    }
}