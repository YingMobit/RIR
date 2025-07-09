using UnityEngine;

public interface IStateMachine {
    /// <summary>
    /// �л���ָ��������ʽö�ٵ�״̬
    /// </summary>
    /// <param name="stateFlag">ö�ٶ�Ӧ������ֵ</param>
    public void SwitchState(int stateFlag);
    public void ReturnToDefualt();
    public abstract GameObject Entity { get; }
}