using System;
using System.Collections.Generic;
using UnityEngine;

public class EventCenter{
    private Dictionary<Type, Delegate> EventPool=new();

    public void Listen<EventType>(Action<EventType> _callback) where EventType : IEvent{
        Type _type = typeof(EventType);
        if(EventPool.ContainsKey(_type)){
            EventPool[_type] = Delegate.Combine(EventPool[_type],_callback);
        } else {
            EventPool.Add(_type,null);
        }
    }

    public void CancelListen<EventType>(Action<EventType> _callback) where EventType : IEvent {
        Type _type = typeof(EventType);
        if(EventPool.ContainsKey(_type)) {
            EventPool[_type] = Delegate.Remove(EventPool[_type],_callback);
        } else {
            Debug.LogError($"Event: {_type} has not been Listened");
        }
    }

    public void Invoke<EventType>(EventType _data) where EventType : IEvent {
        Type _type = typeof(EventType);
        if(EventPool.ContainsKey(_type)){
            Delegate _dele = EventPool[_type];
            if(_dele != null && _dele is Action<EventType> _action && _action != null){
                _action.Invoke(_data);
            }
        } else {
            Debug.LogError($"Event: {_type} has not been published");
        }
    }
}


/// <summary>
/// 抽象接口用于类型规范
/// </summary>
public interface IEvent{ }
