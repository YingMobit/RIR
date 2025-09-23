using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class Attribute<TValue> {
    private TValue baseValue;
    private TValue currentValue;
    private Action<TValue,TValue> onDataChange;
    public TValue CurrentValue => currentValue;
    public TValue BaseValue => baseValue;


    public void AddModifier(AttributeModifier<TValue> modifier) {
        TValue ori = currentValue;
        modifier.Modify(ref currentValue);
        onDataChange?.Invoke(ori,currentValue);
    }

    public Attribute(TValue _base,Action<TValue,TValue> _actionOnDataChange) {
        baseValue = _base;
        currentValue = _base;
        onDataChange = _actionOnDataChange;
    }
}