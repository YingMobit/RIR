namespace GAS {
    /// <summary>
    /// 控制器抽象接口，所有对组件的裸操作都应通过对应的控制器暴露的接口实现
    /// </summary>
    public interface IController {
        public ControllerTypeEnum Type { get; }
    }
}
