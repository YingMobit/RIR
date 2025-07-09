using System.Collections.Generic;
using UnityEngine;

public interface IBuffEvent : IEvent { }
public interface IBuffOnAttackEvent : IBuffEvent { }
public interface IBuffOnBeAttackedEvent : IBuffEvent { }
public interface IBuffOnDeathEvent : IBuffEvent {
    public List<GameObject> Around { get; set; }
}
public interface IBuffOnKillEvent : IBuffEvent { }

//.....