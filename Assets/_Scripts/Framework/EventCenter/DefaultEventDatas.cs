using System;

public interface INullDataEvent : IEventData { }
/// <summary>
/// ����Ҫ���ݲ��������¼������ݽṹ�壬���������͵��¼�ʱ������ṹ��������µ�ʵ����INullDataEvent�Ŀսӿڼ���
/// </summary>
public struct NullEventData : INullDataEvent { }

public interface IValueChangedEvent<TValue> : IEventData {
    TValue OriginValue { get; }
    TValue CurrentValue { get; }
}

/// <summary>
/// ���ֵ�仯�����¼������ݽṹ�壬�����µ��¼�����ʱ��������̳�����ಢ�����½ӿڼ���
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