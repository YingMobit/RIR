using UnityEngine;

public interface IStateMachine {
    /// <summary>
    /// 切换到指定整数形式枚举的状态
    /// </summary>
    /// <param name="stateFlag">枚举对应的整数值</param>
    public void SwitchState(int stateFlag);
    public void ReturnToDefualt();
    public abstract GameObject Entity { get; }
}