using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Buff驱动类，用于驱动Buff生命周期并对外提供接口
/// </summary>
public class BuffHandler : MonoBehaviour {
    private readonly Dictionary<int,Buff> Buffs = new();
    private readonly List<BuffEffectRequest> BuffEffectThisFrame = new();
    private readonly HashSet<Buff> timeOutBuff = new();
    public readonly EventCenter LocalBuffEventCenter = new EventCenter();

    #region 外部调用

    public void AddBuff<TBuff>(int id,BuffRunTimeData runTimeData) where TBuff : Buff, new() {
        if(GetBuff(id) is TBuff buff) {
            //buff存在
            if(buff.ConfigData.UseDefualtStackStrategy) {
                switch(buff.ConfigData.BuffStackUpStrategy) {
                    case BuffStackUpStrategy.ResetRunTime:
                        buff.RunTimeData.RunTime = 0;
                        buff.RunTimeData.Ticks = 0;
                        break;
                    case BuffStackUpStrategy.AddDuration:
                        buff.RunTimeData.ActualDuration += buff.ConfigData.Duration * runTimeData.Stack;
                        break;
                    case BuffStackUpStrategy.AddStackOnly:
                        buff.RunTimeData.Stack += runTimeData.Stack;
                        buff.RunTimeData.Stack %= buff.ConfigData.MaxStack;
                        break;
                    case BuffStackUpStrategy.AddStackAndResetRunTime:
                        buff.RunTimeData.Stack += runTimeData.Stack;
                        buff.RunTimeData.Stack %= buff.ConfigData.MaxStack;
                        buff.RunTimeData.RunTime = 0;
                        buff.RunTimeData.Ticks = 0;
                        break;
                    case BuffStackUpStrategy.Independent:
                        buff.UpperBuffStack(runTimeData.Stack);
                        break;
                }
            } else {
                buff.UpperBuffStack(runTimeData.Stack);
            }
        } else {
            //buff不存在
            buff = BuffFactory.Instance.CreateBuff<TBuff>(id,runTimeData);
            buff.StartBuff();
            Buffs.Add(id,buff);
        }
    }

    /// <summary>
    /// 指定Buff并移除指定层数
    /// </summary>
    /// <param name="id">BuffID</param>
    /// <param name="stackCount">期望移除的层数，默认为1</param>
    public void RemoveBuffStack(int id,int stackCount = 1) {
        Buff buff = GetBuff(id);
        if(buff == null)
            return;

        if(buff.ConfigData.UseDefualtStackStrategy) {
            switch(buff.ConfigData.BuffStackDownStrategy) {
                case BuffStackDownStrategy.Reduce:
                    buff.RunTimeData.RunTime = 0;
                    buff.RunTimeData.Stack -= stackCount;
                    buff.RunTimeData.Ticks = 0;
                    break;
                case BuffStackDownStrategy.Clear:
                    buff.RunTimeData.Stack = 0;
                    break;
                case BuffStackDownStrategy.Independent:
                    buff.DownBuffStack();
                    break;
            }
        } else {
            buff.DownBuffStack(stackCount);
        }

        //在本帧处理过期Buff
        //可能在本对象的Update之后执行，所以这里保留过期处理
        if(buff.IsCompletelyOver)
            timeOutBuff.Add(buff);
    }

    /// <summary>
    /// 直接清除指定buff，不论层数
    /// </summary>
    /// <param name="id">BuffID</param>
    public void ClearBuff(int id) {
        timeOutBuff.Add(GetBuff(id));
    }

    /// <summary>
    /// 清除所有Buff
    /// </summary>
    public void ClearAllBuff() {
        foreach(var buff in Buffs.Values) {
            timeOutBuff.Add(buff);
        }
    }

    /// <summary>
    /// 查询Buff是否存在
    /// </summary>
    /// <param name="id">BuffID</param>
    /// <returns></returns>
    public bool ExistBuff(int id) {
        return GetBuff(id) != null;
    }

    /// <summary>
    /// 获取Buff实例
    /// </summary>
    /// <param name="id">期望的BuffID</param>
    /// <returns></returns>
    public Buff GetBuff(int id) {
        if(Buffs.ContainsKey(id)) {
            return Buffs[id];
        }
        return null;
    }
    #endregion

    /// <summary>
    /// 用于提交Buff生效请求，仅应该暴露给事件型Buff
    /// </summary>
    /// <param name="request">请求实例</param>
    public void RegistBuffEffectRequest(BuffEffectRequest request) {
        if(BuffEffectThisFrame.Count == 0) {
            BuffEffectThisFrame.Add(request);
            return;
        }
        int left = 0, right = BuffEffectThisFrame.Count;
        int mid;

        while(left < right) {
            mid = (left + right) / 2;
            if(BuffEffectThisFrame[mid] < request) {
                left = mid + 1;
            } else {
                right = mid;
            }
        }

        BuffEffectThisFrame.Insert(left,request);
    }

    #region Buff周期处理
    private void Update() {
        UpdateBuffState();
    }

    //清除失效Buff
    private void LateUpdate() {
        if(BuffEffectThisFrame.Count > 0) {
            foreach(var request in BuffEffectThisFrame) {
                request.Invoke();
            }
            BuffEffectThisFrame.Clear();
        }

        if(timeOutBuff.Count > 0) {
            foreach(var buff in timeOutBuff) {
                Buffs.Remove(buff.ConfigData.ID);
                buff.EndBuff();
            }
            timeOutBuff.Clear();
        }
    }

    void UpdateBuffState() {
        foreach(var buff in Buffs.Values) {
            buff.RunTimeData.RunTime += Time.deltaTime;
            if(buff.ConfigData.BuffTriggerType == BuffTriggerType.Tick) {
                //周期性Buff调用
                if(buff.RunTimeData.RunTime < buff.RunTimeData.ActualDuration &&
                    (int)(buff.RunTimeData.RunTime / buff.ConfigData.TickTime) >= buff.RunTimeData.Ticks) {
                    RegistBuffEffectRequest(
                        new(() => buff.ConfigData.BuffEffect.Effect(buff.RunTimeData),
                        buff.ConfigData.Priority));
                    buff.RunTimeData.Ticks++;
                }
            }

            //更新Buff生命周期
            //先处理结束
            if(buff.IsCompletelyOver) {
                timeOutBuff.Add(buff);
                return;
            }

            //再处理降层
            if(buff.StackOver) {
                if(buff.ConfigData.UseDefualtStackStrategy) {
                    switch(buff.ConfigData.BuffStackDownStrategy) {
                        case BuffStackDownStrategy.Reduce:
                            buff.RunTimeData.RunTime = 0;
                            buff.RunTimeData.Stack--;
                            buff.RunTimeData.Ticks = 0;
                            break;
                        case BuffStackDownStrategy.Clear:
                            buff.RunTimeData.Stack = 0;
                            break;

                        case BuffStackDownStrategy.Independent:
                            buff.DownBuffStack();
                            break;
                    }
                } else {
                    buff.DownBuffStack();
                }
            }

            //重新审核
            if(buff.IsCompletelyOver) {
                timeOutBuff.Add(buff);
            }
        }
    }
    #endregion
}

/// <summary>
/// 封装一次Buff生效操作，按优先级排序
/// </summary>
public readonly struct BuffEffectRequest {
    public readonly Action Effect;
    public readonly int Priority;

    public readonly void Invoke() {
        Effect.Invoke();
    }

    public BuffEffectRequest(Action effect,int priority) {
        Effect = effect;
        Priority = priority;
    }

    public static bool operator <(BuffEffectRequest request1,BuffEffectRequest request2) { return request1.Priority < request2.Priority; }
    public static bool operator >(BuffEffectRequest request1,BuffEffectRequest request2) { return request1.Priority > request2.Priority; }
}