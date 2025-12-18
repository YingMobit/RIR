using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace UISystem { 
    public class UIPanel : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerMoveHandler {
        Dictionary<StackUILayer,GameObject> StackUILayerMap = new();
        private HashSet<UIElement> uiElements = new();

        #region Callbacks
        public event Action OnStackDownEvent;
        public event Action OnStackUpEvent;
        public event Action OnUpdate;
        public event Action DisableEvent;
        public event Action EnableEvent;
        public event Action OnUnLoadEvent;
        public event Action<PointerEventData> OnPointerEnterEvent;
        public event Action<PointerEventData> OnPointerDownEvent;
        public event Action<PointerEventData> OnPointerClickEvent;
        public event Action<PointerEventData> OnPointerUpEvent;
        public event Action<PointerEventData> OnPointerExitEvent;
        public event Action<PointerEventData> OnPointerMoveEvent;
        #endregion

        #region LifeTime
        public void InitPanel() {
            var thisRect = GetComponent<RectTransform>();
            var staticLayer = UIManager.Instance.CreateEmptyUIHolder("StaticLayer",thisRect);
            var activeLayer = UIManager.Instance.CreateEmptyUIHolder("ActiveLayer",thisRect);
            var floatingLayer = UIManager.Instance.CreateEmptyUIHolder("FloatingLayer",thisRect);

            StackUILayerMap[StackUILayer.Static] = staticLayer;
            StackUILayerMap[StackUILayer.Active] = activeLayer;
            StackUILayerMap[StackUILayer.Floating] = floatingLayer;
        }

        public void OnStackDown() => OnStackDownEvent?.Invoke();

        public void OnStackUp() => OnStackUpEvent?.Invoke();

        public void Disable() => DisableEvent?.Invoke();

        public void Enable() => EnableEvent?.Invoke();

        public void OnUnLoad() {
            OnUnLoadEvent?.Invoke();
            OnStackDownEvent = null;
            OnStackUpEvent = null;
            OnUpdate = null;
            DisableEvent = null;
            EnableEvent = null;
            OnUnLoadEvent = null;
            OnPointerEnterEvent = null;
            OnPointerDownEvent = null;
            OnPointerClickEvent = null;
            OnPointerUpEvent = null;
            OnPointerExitEvent = null;
            OnPointerMoveEvent = null;
        }

        public void UpdateUIPanel() {
            OnUpdate?.Invoke();
            foreach(var uiElement in uiElements) {
                uiElement.UpdateElement();
            }
        }
        #endregion

        #region API
        public UIElement LoadUIElement(UIPrefabInfo uiPrefabInfo,StackUILayer stackUILayer,Action<UIElement> initialize = null) {
            if(!UIPrefabLoader.Instance.TryLoadUI(uiPrefabInfo,out var ui))
                return null;

            var uiElement = ui.GetComponent<UIElement>();
            if(!uiElement) {
                Debug.LogError($"LoadUIElement Fail: {uiPrefabInfo.UIPrefab.name} dont have UIElement Component");
                UIPrefabLoader.Instance.UnloadUI(ui);
                return null;
            }

            uiElement.BindUIPanel(this);
            initialize?.Invoke(uiElement);
            ui.transform.SetParent(StackUILayerMap[stackUILayer].transform, false);
            
            uiElements.Add(uiElement);
            return uiElement;
        }

        public void UnLoadUIElement(UIElement uiElement) {
            uiElement.Disable();
            uiElement.Unload();
            UIPrefabLoader.Instance.UnloadUI(uiElement.gameObject);
            uiElements.Remove(uiElement);
        }

        public void UnLoadAllUIElements() {
            var copy = ListPool<UIElement>.Get();
            if(copy.Capacity < uiElements.Count) copy.Capacity = uiElements.Count;
            copy.AddRange(uiElements);
            foreach(var uiElement in copy) {
                UnLoadUIElement(uiElement);
            }
            uiElements.Clear();
            ListPool<UIElement>.Release(copy);
        }
        #endregion

        #region PointerEvents
        public void OnPointerClick(PointerEventData eventData) => OnPointerClickEvent?.Invoke(eventData);

        public void OnPointerDown(PointerEventData eventData) => OnPointerDownEvent?.Invoke(eventData);

        public void OnPointerEnter(PointerEventData eventData) => OnPointerEnterEvent?.Invoke(eventData);

        public void OnPointerExit(PointerEventData eventData) => OnPointerExitEvent?.Invoke(eventData);

        public void OnPointerUp(PointerEventData eventData) => OnPointerUpEvent?.Invoke(eventData);

        public void OnPointerMove(PointerEventData eventData) => OnPointerMoveEvent?.Invoke(eventData);
        #endregion
    }
}