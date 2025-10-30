using ReferencePoolingSystem;
using System;
using UnityEngine;

namespace GAS {
    /// <summary>
    /// 控制器抽象接口，所有对组件的裸操作都应通过对应的控制器暴露的接口实现
    /// </summary>
    public interface IController {
        public ControllerTypeEnum Type { get; }
        public GameObject GameObject { get; }
        public void BindGameObject(GameObject gameObject);
        public void Update();
        public void LateUpdate();
    }
}
