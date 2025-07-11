using System;
using Utility;

public class GlobalEventCenter : Singleton<GlobalEventCenter> {
    private EventCenter _eventCenter = new EventCenter();

    public void Listen<EventType>(Action<EventType> callback) where EventType : IEventData {
        _eventCenter.Listen(callback);
    }

    public void CancelListen<EventType>(Action<EventType> callback) where EventType : IEventData {
        _eventCenter.CancelListen(callback);
    }

    public void Invoke<EventType>(EventType data) where EventType : IEventData {
        _eventCenter.Invoke(data);
    }
}