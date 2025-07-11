using UnityEngine;

/// <summary>
/// 具体的Buff效果由这个类的派生类实现
/// </summary>
public abstract class BuffEffect : ScriptableObject {
    /// <summary>
    /// Buff效果的实现函数
    /// </summary>
    /// <param name="runTimeData">运行时数据</param>
    /// <param name="eventData">事件型buff所需的事件信息</param>
    public abstract void Effect(BuffRunTimeData runTimeData,IEventData eventData = null);

    public abstract BuffEffect Clone();
}