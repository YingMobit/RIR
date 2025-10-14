using System;
using UnityEngine;

public class ValueSmoothTask<TValue> where TValue : struct , IEquatable<TValue> {
    private TValue _currentValue;
    private TValue _initValue;
    private TValue _targetValue;
    private float _smoothTime;
    private float _startTime;

    private Action<TValue> _applyData;
    private Func<TValue,TValue,float,TValue> _lerpMethod;
    public void RegistTask(TValue initialValue,TValue targetValue, float smoothTime,Action<TValue> applyData,Func<TValue, TValue, float, TValue> lerpMethod)
    {
        _initValue = initialValue;
        if(targetValue.Equals(_targetValue) && smoothTime.Equals(_smoothTime)) {
            _targetValue = targetValue;
        } else { 
            _targetValue = targetValue;
            _smoothTime = smoothTime;
            _startTime = Time.time;
            _initValue = initialValue;   
        }

        _applyData = applyData;
        _lerpMethod = lerpMethod;
    }

    public void SetTargetValue(TValue targetValue)
    {
        _targetValue = targetValue;
    }

    public bool Update()
    {
        _currentValue = _lerpMethod.Invoke(_initValue, _targetValue, Mathf.Clamp01((Time.time - _startTime) / _smoothTime));
        _applyData?.Invoke(_targetValue);
        if(Time.time - _startTime > _smoothTime)
            return true;
        return false;
    }

    public TValue GetCurrentValue()
    {
        return _currentValue;
    }
}