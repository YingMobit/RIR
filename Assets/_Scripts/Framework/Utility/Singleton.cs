using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Utility {
    public class Singleton<T> : MonoBehaviour where T : Singleton<T> {
        private static T _instance;
        public static T Instance {
            get {
                if(_instance == null || _instance.IsUnityNull()) {
                    _instance = FindAnyObjectByType<T>();
                    if(_instance == null || _instance.IsUnityNull()) {
                        _instance = CreateInstance?.Invoke();
                    }
                }
                return _instance;
            }
        }

        #region 自定义行为
        protected virtual bool _isDonDestroyOnLoad => false;

        /// <summary>
        /// 可重写的创建逻辑
        /// </summary>
        protected static Func<T> CreateInstance { get; set; } = () => {
            GameObject _go = new GameObject(typeof(T).Name);
            T _res = _go.AddComponent<T>();
            return _res;
        };
        #endregion

        #region 生命周期
        protected virtual void Awake() {
            if(_instance == null || _instance.IsUnityNull()) {
                _instance = this as T;
            } else if(_instance != this) {
                DestroyImmediate(gameObject);
                return;
            }

            if(_isDonDestroyOnLoad) {
                DontDestroyOnLoad(gameObject);
            }
        }
        #endregion
    }
}
