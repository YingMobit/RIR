using UnityEngine;

public interface IFactory<ObjectType>
{
    public ObjectType CreateInstance();
}
