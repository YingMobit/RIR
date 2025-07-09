using System;

/// <summary>
/// 抽象Buff类，用于Buff的生命周期管理
/// </summary>
public abstract class Buff {
    public BuffConfigData ConfigData;
    public BuffRunTimeData RunTimeData;
    /// <summary>
    /// 指示Buff是否应该在本帧结束
    /// </summary>
    public virtual bool IsCompletelyOver => !ConfigData.isForever && RunTimeData.Stack <= 0;

    /// <summary>
    /// 指示Buff本帧是否应该降层
    /// </summary>
    public virtual bool StackOver => !ConfigData.isForever && RunTimeData.RunTime > RunTimeData.ActualDuration;

    /// <summary>
    /// 当Buff被创建到对象时调用，整个Buff生命周期只会调用一次
    /// </summary>
    public abstract void StartBuff();
    /// <summary>
    /// 当Buff层数增加时调用
    /// </summary>
    /// <param name="stackCount">Buff增加的层数，默认为一层</param>
    public abstract void UpperBuffStack(int stackCount = 1);
    /// <summary>
    /// 对事件触发型Buff适用
    /// </summary>
    /// <param name="eventData"></param>
    protected abstract void TriggerBuff(IEvent eventData);
    /// <summary>
    /// 当Buff层数降低时调用
    /// </summary>
    /// <param name="stackCount">Buff降低的层数，默认为一</param>
    /// <returns>返回降低之后的Buff层数</returns>
    public abstract int DownBuffStack(int stackCount = 1);
    /// <summary>
    /// 当Buff结束时调用
    /// </summary>
    public abstract void EndBuff();
}