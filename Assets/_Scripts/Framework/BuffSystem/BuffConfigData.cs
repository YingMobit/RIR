using System.Collections.Generic;
using BuffSystem.Config;
using UnityEditor.EditorTools;
using UnityEngine;

/// <summary>
/// 通用的Buff配置数据类
/// </summary>
[CreateAssetMenu(fileName = "BuffConfigData",menuName = BuffSystemConfig.BuffConfigDataMenuPath,order = 1)]
public class BuffConfigData : ScriptableObject {
    //描述性数据
    public int ID;
    public string Name;
    public string Description;
    [Tooltip("图标")]
    public Sprite Icon;
    [Tooltip("数字越低优先级越高")]
    public int Priority;
    [Tooltip("Buff标签")]
    public List<BuffTag> Tags;

    //生命周期配置
    [Tooltip("最大层数")]
    public int MaxStack;
    [Tooltip("是否是永久buff")]
    public bool isForever;
    [Tooltip("Buff持续时间")]
    public float Duration;
    [Tooltip("Buff触发类型")]
    public BuffTriggerType BuffTriggerType;
    [Tooltip("周期型buff的触发周期")]
    public float TickTime;
    [Tooltip("是否使用预制的buff层数处理逻辑")]
    public bool UseDefualtStackStrategy;
    [Tooltip("层数增加时的预制处理逻辑")]
    public BuffStackUpStrategy BuffStackUpStrategy;
    [Tooltip("层数减少时的预制处理逻辑")]
    public BuffStackDownStrategy BuffStackDownStrategy;
    [Tooltip("当层数影响Buff持续时间时,每一层增加多少时间")]
    public float DurationExtendPerStack;

    // Buff功能配置
    public BuffEffect BuffEffect;

    /// <summary>
    /// 将数据拷贝到新的ConfigData中，防止污染配置数据
    /// </summary>
    /// <param name="emptyData"></param>
    public virtual void CopyTo(BuffConfigData emptyData) {
        emptyData.ID = ID;
        emptyData.Name = Name;
        emptyData.Description = Description;
        emptyData.Icon = Icon;
        emptyData.Priority = Priority;
        emptyData.Tags = Tags;
        emptyData.MaxStack = MaxStack;
        emptyData.isForever = isForever;
        emptyData.Duration = Duration;
        emptyData.TickTime = TickTime;
        emptyData.UseDefualtStackStrategy = UseDefualtStackStrategy;
        emptyData.BuffStackUpStrategy = BuffStackUpStrategy;
        emptyData.BuffStackDownStrategy = BuffStackDownStrategy;
        emptyData.DurationExtendPerStack = DurationExtendPerStack;
        emptyData.BuffEffect = BuffEffect;
    }
}