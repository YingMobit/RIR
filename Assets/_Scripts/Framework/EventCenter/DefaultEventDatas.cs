using System;

public interface INullDataEvent : IEventData { }
/// <summary>
/// 不需要传递参数类型事件的数据结构体，当有新类型的事件时在这个结构体上添加新的实现了INullDataEvent的空接口即可
/// </summary>
public struct NullEventData : INullDataEvent { }

public interface IValueChangedEvent<TValue> : IEventData {
    TValue OriginValue { get; }
    TValue CurrentValue { get; }
}

/// <summary>
/// 监控值变化类型事件的数据结构体，当有新的事件类型时创建新类继承这个类并创建新接口即可
/// </summary>
/// <typeparam name="TValue"></typeparam>
public class ValueChangedEventData<TValue> : IValueChangedEvent<TValue> {
    private TValue originValue;
    private TValue currentValue;

    public TValue OriginValue => originValue;

    public TValue CurrentValue => currentValue;
    public ValueChangedEventData(TValue origin,TValue current) {
        originValue = origin;
        currentValue = current;
    }
}