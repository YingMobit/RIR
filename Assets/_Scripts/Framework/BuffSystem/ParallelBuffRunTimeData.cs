using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 串行Buff运行时数据，继承自BuffRunTimeData
/// </summary>
public class ParallelBuffRunTimeData : BuffRunTimeData {
    private Dequeue<float> quitTimeStamp;
    private float lastEntryTime;
    public override float ActualDuration { get => quitTimeStamp.PickHead(); set => base.ActualDuration = value; }

    public ParallelBuffRunTimeData(GameObject from,GameObject owner,int stack) : base(from,owner,stack) {
        quitTimeStamp = new();
    }

    public void StackEntry(float runTime) {
        float extend = runTime - lastEntryTime;
        quitTimeStamp.PushTail(quitTimeStamp.PickTail() + extend);
        lastEntryTime = runTime;
    }

    public void StackExit() {
        quitTimeStamp.PopHead();
    }

    public void Init(float initDuration) {
        lastEntryTime = 0;
        quitTimeStamp.PushTail(initDuration);
    }
}

public class Dequeue<TData> : List<TData> {
    public TData PopHead() {
        if(Count != 0) {
            TData data = base[0];
            RemoveAt(0);
            return data;
        } else {
            return default;
        }
    }

    public TData PickHead() {
        if(Count != 0) {
            return base[0];
        } else {
            return default;
        }
    }

    public TData PopTail() {
        if(Count != 0) {
            TData data = base[Count - 1];
            RemoveAt(Count - 1);
            return data;
        } else {
            return default;
        }
    }

    public TData PickTail() {
        if(Count != 0) {
            return base[Count - 1];
        } else {
            return default;
        }
    }

    public void PushHead(TData data) {
        Insert(0,data);
    }

    public void PushTail(TData data) {
        Add(data);
    }
}