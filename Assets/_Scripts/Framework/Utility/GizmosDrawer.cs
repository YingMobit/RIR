using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utility {
    public class GizmosDrawer : Singleton<GizmosDrawer> {
        // 存储所有需要绘制的Gizmos回调
        private List<Action> _gizmosDrawCallbacks = new List<Action>();
        private List<Action> _gizmosSelectedDrawCallbacks = new List<Action>();

        /// <summary>
        /// 注册Gizmos绘制回调
        /// </summary>
        public void RegisterGizmosDrawer(Action drawCallback) {
            if(drawCallback != null && !_gizmosDrawCallbacks.Contains(drawCallback)) {
                _gizmosDrawCallbacks.Add(drawCallback);
            }
        }

        /// <summary>
        /// 注销Gizmos绘制回调
        /// </summary>
        public void UnregisterGizmosDrawer(Action drawCallback) {
            _gizmosDrawCallbacks.Remove(drawCallback);
        }

        /// <summary>
        /// 注册选中时的Gizmos绘制回调
        /// </summary>
        public void RegisterGizmosSelectedDrawer(Action drawCallback) {
            if(drawCallback != null && !_gizmosSelectedDrawCallbacks.Contains(drawCallback)) {
                _gizmosSelectedDrawCallbacks.Add(drawCallback);
            }
        }

        /// <summary>
        /// 注销选中时的Gizmos绘制回调
        /// </summary>
        public void UnregisterGizmosSelectedDrawer(Action drawCallback) {
            _gizmosSelectedDrawCallbacks.Remove(drawCallback);
        }

        private void OnDrawGizmos() {
            foreach(var callback in _gizmosDrawCallbacks) {
                try {
                    callback?.Invoke();
                } catch(Exception e) {
                    Debug.LogError($"[GizmosDrawer] Error in OnDrawGizmos: {e}");
                }
            }
        }

        private void OnDrawGizmosSelected() {
            foreach(var callback in _gizmosSelectedDrawCallbacks) {
                try {
                    callback?.Invoke();
                } catch(Exception e) {
                    Debug.LogError($"[GizmosDrawer] Error in OnDrawGizmosSelected: {e}");
                }
            }
        }

        private void OnDestroy() {
            _gizmosDrawCallbacks.Clear();
            _gizmosSelectedDrawCallbacks.Clear();
        }
    }
}