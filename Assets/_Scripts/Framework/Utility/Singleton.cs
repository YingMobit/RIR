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
                        if(!_instance.awaked)
                            _instance.Awake();
                    }
                }
                return _instance;
            }
        }

        #region �Զ�����Ϊ
        protected virtual bool _isDonDestroyOnLoad => false;

        /// <summary>
        /// ����д�Ĵ����߼�
        /// </summary>
        protected static Func<T> CreateInstance { get; set; } = () => {
            GameObject _go = new GameObject(typeof(T).Name);
            T _res = _go.AddComponent<T>();
            return _res;
        };
        #endregion

        #region ��������
        protected bool awaked = false;
        protected virtual void Awake() {
            if(awaked)
                return;
            if(_instance == null || _instance.IsUnityNull()) {
                _instance = this as T;
            } else if(_instance != this) {
                DestroyImmediate(gameObject);
                return;
            }

            if(_isDonDestroyOnLoad) {
                DontDestroyOnLoad(gameObject);
            }
            awaked = true;
        }
        #endregion
    }
}
