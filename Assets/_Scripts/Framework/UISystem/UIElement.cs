using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UISystem { 
    public class UIElement : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerMoveHandler {
        private UIPanel m_uipanel;
        public UIPanel UIPanel => m_uipanel;

        #region LifeTime
        public void BindUIPanel(UIPanel uiPanel) { 
            m_uipanel = uiPanel;
        }

        public void Enable() => OnEnableEvent?.Invoke();

        public void Disable() => OnDisableEvent?.Invoke();

        public void UpdateElement() => OnUpdateEvent?.Invoke();
        
        public void Unload() {
            OnUnloadEvent?.Invoke();
            OnEnableEvent = null;
            OnUpdateEvent = null;
            OnDisableEvent = null;
            OnUnloadEvent = null;
            OnPointerEnterEvent = null;
            OnPointerDownEvent = null;
            OnPointerClickEvent = null;
            OnPointerUpEvent = null;
            OnPointerExitEvent = null;
            OnPointerMoveEvent = null;
        }
        #endregion

        #region Callbacks
        public event Action OnEnableEvent;
        public event Action OnUpdateEvent;
        public event Action OnDisableEvent;
        public event Action OnUnloadEvent;
        public event Action<PointerEventData> OnPointerEnterEvent;
        public event Action<PointerEventData> OnPointerDownEvent;
        public event Action<PointerEventData> OnPointerClickEvent;
        public event Action<PointerEventData> OnPointerUpEvent;
        public event Action<PointerEventData> OnPointerExitEvent;
        public event Action<PointerEventData> OnPointerMoveEvent;
        #endregion

        #region Pointer Events
        public void OnPointerClick(PointerEventData eventData) => OnPointerClickEvent?.Invoke(eventData);

        public void OnPointerDown(PointerEventData eventData) => OnPointerDownEvent?.Invoke(eventData);

        public void OnPointerEnter(PointerEventData eventData) => OnPointerEnterEvent?.Invoke(eventData);

        public void OnPointerExit(PointerEventData eventData) => OnPointerExitEvent?.Invoke(eventData);

        public void OnPointerUp(PointerEventData eventData) => OnPointerUpEvent?.Invoke(eventData);

        public void OnPointerMove(PointerEventData eventData) => OnPointerMoveEvent?.Invoke(eventData);
        #endregion
    }
}