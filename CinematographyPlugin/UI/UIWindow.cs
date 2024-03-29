﻿using CinematographyPlugin.UI.Enums;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace CinematographyPlugin.UI
{
    public class UIWindow : MonoBehaviour
    {
        private bool _aspectRatioOn;
        private float _lastCanvasAlpha;
        private RectTransform _dragRectTransform;
        private CanvasGroup _canvasGroup;
        private Canvas _canvas;
        
        private void Awake()
        {
            _dragRectTransform = transform.GetComponent<RectTransform>();
            _canvas = transform.GetComponentInParent<Canvas>();
            _canvasGroup = _canvas.GetComponentInParent<CanvasGroup>();
            _lastCanvasAlpha = _canvasGroup.alpha;

            var trigger = GetComponent<EventTrigger>();

            var drag = new EventTrigger.Entry();
            drag.eventID = EventTriggerType.Drag;
            drag.callback.AddListener((UnityAction<BaseEventData>) OnDrag);
            trigger.triggers.Add(drag);
            
            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((UnityAction<BaseEventData>) OnEnter);
            trigger.triggers.Add(entry);
            
            var exit = new EventTrigger.Entry();
            exit.eventID = EventTriggerType.PointerExit;
            exit.callback.AddListener((UnityAction<BaseEventData>) OnExit);
            trigger.triggers.Add(exit);
        }

        private void Start()
        {
            CinemaUIManager.Current.Toggles[UIOption.ToggleAspectRatio].OnValueChanged += OnToggleAspectRatio;
        }

        public void OnDrag(BaseEventData data)
        {
            _dragRectTransform.anchoredPosition += data.TryCast<PointerEventData>().delta / _canvas.scaleFactor;

            var rect = _dragRectTransform;
            var apos = rect.anchoredPosition;
            var xpos = apos.x;
            xpos = Mathf.Clamp(xpos, 0, Screen.width - rect.sizeDelta.x);
            apos.x = xpos;
            rect.anchoredPosition = apos;
        }
        
        public void OnEnter(BaseEventData data)
        {
            _canvasGroup.alpha = 1;
        }
        
        public void OnExit(BaseEventData data)
        {
            _canvasGroup.alpha = _aspectRatioOn ? 1 : _lastCanvasAlpha;
        }

        public void OnPointerDown(PointerEventData data)
        {
            _dragRectTransform.SetAsLastSibling();
        }

        private void OnToggleAspectRatio(bool value)
        {
            _canvasGroup.alpha = value ? 1 : _lastCanvasAlpha;
            _aspectRatioOn = value;
        }

        private void OnDestroy()
        {
            CinemaUIManager.Current.Toggles[UIOption.ToggleAspectRatio].OnValueChanged -= OnToggleAspectRatio;
        }
    }
}