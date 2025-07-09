using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Buff�����࣬��������Buff�������ڲ������ṩ�ӿ�
/// </summary>
public class BuffHandler : MonoBehaviour {
    private readonly Dictionary<int,Buff> Buffs = new();
    private readonly List<BuffEffectRequest> BuffEffectThisFrame = new();
    private readonly HashSet<Buff> timeOutBuff = new();
    public readonly EventCenter LocalBuffEventCenter = new EventCenter();

    #region �ⲿ����

    public void AddBuff<TBuff>(int id,BuffRunTimeData runTimeData) where TBuff : Buff, new() {
        if(GetBuff(id) is TBuff buff) {
            //buff����
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
            //buff������
            buff = BuffFactory.Instance.CreateBuff<TBuff>(id,runTimeData);
            buff.StartBuff();
            Buffs.Add(id,buff);
        }
    }

    /// <summary>
    /// ָ��Buff���Ƴ�ָ������
    /// </summary>
    /// <param name="id">BuffID</param>
    /// <param name="stackCount">�����Ƴ��Ĳ�����Ĭ��Ϊ1</param>
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

        //�ڱ�֡�������Buff
        //�����ڱ������Update֮��ִ�У��������ﱣ�����ڴ���
        if(buff.IsCompletelyOver)
            timeOutBuff.Add(buff);
    }

    /// <summary>
    /// ֱ�����ָ��buff�����۲���
    /// </summary>
    /// <param name="id">BuffID</param>
    public void ClearBuff(int id) {
        timeOutBuff.Add(GetBuff(id));
    }

    /// <summary>
    /// �������Buff
    /// </summary>
    public void ClearAllBuff() {
        foreach(var buff in Buffs.Values) {
            timeOutBuff.Add(buff);
        }
    }

    /// <summary>
    /// ��ѯBuff�Ƿ����
    /// </summary>
    /// <param name="id">BuffID</param>
    /// <returns></returns>
    public bool ExistBuff(int id) {
        return GetBuff(id) != null;
    }

    /// <summary>
    /// ��ȡBuffʵ��
    /// </summary>
    /// <param name="id">������BuffID</param>
    /// <returns></returns>
    public Buff GetBuff(int id) {
        if(Buffs.ContainsKey(id)) {
            return Buffs[id];
        }
        return null;
    }
    #endregion

    /// <summary>
    /// �����ύBuff��Ч���󣬽�Ӧ�ñ�¶���¼���Buff
    /// </summary>
    /// <param name="request">����ʵ��</param>
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

    #region Buff���ڴ���
    private void Update() {
        UpdateBuffState();
    }

    //���ʧЧBuff
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
                //������Buff����
                if(buff.RunTimeData.RunTime < buff.RunTimeData.ActualDuration &&
                    (int)(buff.RunTimeData.RunTime / buff.ConfigData.TickTime) >= buff.RunTimeData.Ticks) {
                    RegistBuffEffectRequest(
                        new(() => buff.ConfigData.BuffEffect.Effect(buff.RunTimeData),
                        buff.ConfigData.Priority));
                    buff.RunTimeData.Ticks++;
                }
            }

            //����Buff��������
            //�ȴ������
            if(buff.IsCompletelyOver) {
                timeOutBuff.Add(buff);
                return;
            }

            //�ٴ�����
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

            //�������
            if(buff.IsCompletelyOver) {
                timeOutBuff.Add(buff);
            }
        }
    }
    #endregion
}

/// <summary>
/// ��װһ��Buff��Ч�����������ȼ�����
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