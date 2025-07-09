using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Buff运行时所需数据，可根据不同的Buff种类继承并添加新的参数
/// </summary>
public class BuffRunTimeData {
    public GameObject From;//Buff造成者
    public GameObject Owner;//Buff执行者
    public virtual float ActualDuration { get; set; }//实际的Buff运行时间上限
    public float RunTime;//Buff运行时间
    public int Ticks;//下一个运行周期数
    public int Stack;//层数

    public BuffRunTimeData(GameObject from,GameObject owner,int stack) {
        From = from;
        Owner = owner;
        Stack = stack;
        RunTime = 0;
        Ticks = 0;
    }
}