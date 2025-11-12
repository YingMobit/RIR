using System;
using UnityEngine;

public class AttributeSmoothTask<TValue> where TValue : struct, IEquatable<TValue> {
    // 任务ID(用于在Handler中查找)
    public int TaskID { get; private set; }

    // 逻辑值(用于回滚和游戏逻辑)
    private TValue _logicValue;
    public TValue LogicValue => _logicValue;

    // 表现值(用于视觉显示)
    private TValue _visualValue;
    public TValue VisualValue => _visualValue;
    
    // 平滑起始值(固定不变,用于线性插值)
    private TValue _startValue;

    // 平滑配置
    private int _smoothFrames;  // 期望多少帧完成平滑
    private int _elapsedFrames; // 已经过的帧数
    private bool _isActive;       // 是否激活平滑
    private bool _isInitialized;  // 是否已初始化

    // 插值方法和应用方法
    private Func<TValue,TValue,float,TValue> _lerpMethod;
    Func<TValue,TValue,bool> _equal;
    private Action<TValue> _applyVisualValue;

    /// <summary>
    /// 注册或更新平滑任务
    /// </summary>
    /// <param name="taskID">任务ID</param>
    /// <param name="logicValue">目标逻辑值</param>
    /// <param name="smoothFrames">期望多少帧完成平滑</param>
    /// <param name="applyVisualValue">应用视觉值的回调</param>
    /// <param name="lerpMethod">插值方法</param>
    public void RegistTask(
        int taskID,
        TValue logicValue,
        int smoothFrames,
        Action<TValue> applyVisualValue,
        Func<TValue,TValue,float,TValue> lerpMethod,
        Func<TValue,TValue,bool> equal) {
        
        if (!_isInitialized) {
            _visualValue = logicValue;
            _isInitialized = true;
        }

        TaskID = taskID;
        _smoothFrames = Math.Max(1, smoothFrames);
        _applyVisualValue = applyVisualValue;
        _lerpMethod = lerpMethod;
        _equal = equal;

        _logicValue = logicValue;
        _startValue = _visualValue;
        
        _elapsedFrames = 0;
        _isActive = true;
    }

    public void UpdateLogicValue(TValue logicValue, int smoothFrames) {
        _logicValue = logicValue;
        _smoothFrames = Math.Max(1, smoothFrames);
        _startValue = _visualValue;
        _elapsedFrames = 0;
        _isActive = true;
    }

    /// <summary>
    /// 强制同步视觉值到逻辑值(回滚时调用)
    /// </summary>
    public void SyncVisualToLogic() {
        _visualValue = _logicValue;
        _startValue = _logicValue;
        _elapsedFrames = 0;
        _applyVisualValue?.Invoke(_visualValue);
    }

    /// <summary>
    /// 每帧更新平滑
    /// </summary>
    /// <returns>是否已经到达目标(可以移除任务)</returns>
    public bool Update(float deltaTime) {
        if(!_isActive)
            return false;

        _elapsedFrames += 1;

        float t = Mathf.Clamp01((float)_elapsedFrames / (float)_smoothFrames);

        _visualValue = _lerpMethod.Invoke(_startValue, _logicValue, t);
        _applyVisualValue?.Invoke(_visualValue);

        if(_elapsedFrames >= _smoothFrames || _equal(_visualValue,_logicValue)) {
            _visualValue = _logicValue;
            _applyVisualValue?.Invoke(_visualValue);
            _isActive = false;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 重置任务
    /// </summary>
    public void Reset() {
        TaskID = 0;
        _isActive = false;
        _isInitialized = false;
        _elapsedFrames = 0;
        _applyVisualValue = null;
        _lerpMethod = null;
    }
}