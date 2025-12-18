using System;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace UISystem {
    public class UIManager : Singleton<UIManager> {
        protected override bool _isDonDestroyOnLoad => true;
        private GameObject stackUIRoot = null;
        private GameObject bubbleUIRoot = null;
        private Stack<UIPanel> stackUIPanels = new Stack<UIPanel>();

        #region API

        protected override void Awake() {
            base.Awake();
            var thisRect =  transform as RectTransform;
            stackUIRoot = CreateEmptyUIHolder("StackUIRoot",thisRect);
            bubbleUIRoot = CreateEmptyUIHolder("BubbleUIRoot",thisRect);
            stackUIPanels = new Stack<UIPanel>();
        }

        public GameObject CreateEmptyUIHolder(string name,RectTransform parent) {
            var res = new GameObject(name); 
            Destroy(res.GetComponent<Transform>());
            var rect = res.AddComponent<RectTransform>();
            rect.SetParent(parent,false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.localScale = Vector3.one;
            return res;
        }

        public UIPanel LoadUIPanel(UIPrefabInfo uiPrefabInfo,Action<UIPanel> initializer = null) {
            if(!UIPrefabLoader.Instance.TryLoadUI(uiPrefabInfo,out var instance)) 
                return null;
            var uipanel = instance.GetComponent<UIPanel>();
            if(!uipanel) {
                Debug.LogError($"LoadUIElement Fail: {uiPrefabInfo.UIPrefab.name} dont have UIPanel Component");
                UIPrefabLoader.Instance.UnloadUI(instance);
                return null;
            }
            initializer?.Invoke(uipanel);
            
            foreach(var stackPanel in stackUIPanels) {
                stackPanel.OnStackDown();
            }

            if(stackUIPanels.TryPeek(out var panel)) {
                panel.Disable();
            }

            stackUIPanels.Push(uipanel);
            
            return uipanel;
        }

        public void PopUIPanel() {  
            
        }
        #endregion
    }
}
