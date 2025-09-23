using UnityEngine;

public class PlayerRuntimeAttributeData {
    [Header("移动")]
    [SerializeField] public Attribute<float> WalkSpeed;
    [SerializeField] public Attribute<float> RunSpeed;
    [SerializeField] public Attribute<float> JumpHeight;
    [SerializeField] public Attribute<float> JumpVerticalImpulse;
    public PlayerController playerController;

    public PlayerRuntimeAttributeData(PlayerAttributeConfigData configData) {
        WalkSpeed = new(configData.WalkSpeed,(ori,cur) => {
            GlobalEventCenter.Instance.Invoke<IPlayerWalkSpeedChanged>(new PlayerWalkSpeedChangedData(ori,cur));
        });
        RunSpeed = new(configData.RunSpeed,(ori,cur) => {
            GlobalEventCenter.Instance.Invoke<IPlayerRunSpeedChanged>(new PlayerRunSpeedChangedData(ori,cur));
        });
        JumpHeight = new(configData.JumpHeight,(ori,cur) => {
            GlobalEventCenter.Instance.Invoke<IPlayerJumpHeightChanged>(new PlayerJumpHeightChangedData(ori,cur));
        });
        JumpVerticalImpulse = new(configData.JumpVerticalImpulse,(ori,cur) => {
            GlobalEventCenter.Instance.Invoke<IPlayerJumpVerticalImpulseChanged>(new PlayerJumpVerticalImpulseChangedData(ori,cur));
        });
    }
}
#region 事件相关接口和类
public interface IPlayerWalkSpeedChanged : IValueChangedEvent<float> { }
public class PlayerWalkSpeedChangedData : ValueChangedEventData<float>, IPlayerWalkSpeedChanged {
    public PlayerWalkSpeedChangedData(float ori,float cur) : base(ori,cur) { }
}
public interface IPlayerRunSpeedChanged : IValueChangedEvent<float> { }
public class PlayerRunSpeedChangedData : ValueChangedEventData<float>, IPlayerRunSpeedChanged {
    public PlayerRunSpeedChangedData(float ori,float cur) : base(ori,cur) { }
}
public interface IPlayerJumpHeightChanged : IValueChangedEvent<float> { }
public class PlayerJumpHeightChangedData : ValueChangedEventData<float>, IPlayerJumpHeightChanged {
    public PlayerJumpHeightChangedData(float ori,float cur) : base(ori,cur) { }
}
public interface IPlayerJumpVerticalImpulseChanged : IValueChangedEvent<float> { }
public class PlayerJumpVerticalImpulseChangedData : ValueChangedEventData<float>, IPlayerJumpVerticalImpulseChanged {
    public PlayerJumpVerticalImpulseChangedData(float ori,float cur) : base(ori,cur) { }
}
#endregion

