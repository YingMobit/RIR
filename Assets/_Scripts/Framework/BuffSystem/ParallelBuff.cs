using System;
using TMPro;
using UnityEngine;

/// <summary>
/// 并行Buff基类，使用串行模拟并行
/// </summary>
public class ParallelBuff : Buff {
    private ParallelBuffRunTimeData _parallelData;
    private ParallelBuffRunTimeData ParallelData {
        get {
            _parallelData ??= RunTimeData as ParallelBuffRunTimeData;
            return _parallelData;
        }
    }

    // public override bool StackOver => !ConfigData.isForever && ;

    public override void StartBuff() {
        ParallelData.Init(ConfigData.Duration);
    }

    public override void UpperBuffStack(int stackCount = 1) {
        ParallelData.Stack += stackCount;
        ParallelData.StackEntry(RunTimeData.RunTime);
    }

    public override int DownBuffStack(int stackCount = 1) {
        ParallelData.Stack -= stackCount;
        ParallelData.StackExit();
        return RunTimeData.Stack;
    }

    public override void EndBuff() {

    }

    protected override void TriggerBuff(IEvent eventData) {

    }
}
