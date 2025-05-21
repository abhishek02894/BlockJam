using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tag.Block
{
    public class InputManager : Manager<InputManager>
    {
        #region private veriables
        [SerializeField] private Camera defaultCamera;
        [SerializeField] private bool convertToWorldSpace;

        private List<Action<Vector3>> mouseButtonDownEvent = new List<Action<Vector3>>();
        private List<Action<Vector3>> mouseButtonUpEvent = new List<Action<Vector3>>();
        private List<Action<Vector3>> mouseButtonMoveEvent = new List<Action<Vector3>>();
        private List<Action<Vector3>> onUIClick = new List<Action<Vector3>>();
        private List<Action<Vector3>> onUIPointerUp = new List<Action<Vector3>>();

        private bool isMouseDown;

        private Touch touch;
        #endregion

        #region propertices
        public static Transform eventTranform
        {
            get
            {
                return Instance.defaultCamera.transform;
            }
        }

        public static Camera EventCamera
        {
            get
            {
                return Instance.defaultCamera;
            }
        }

        public bool CanMoveCamera { get; set; }

        public static bool RaycastBlock
        {
            get { return BaseView.blockView.Count > 0; }
        }

        public static bool StopInteraction
        {
            get;
            set;
        }
        public bool IsMouseDown { get => isMouseDown; private set => isMouseDown = value; }
        #endregion

        #region unity callback

        private void Start()
        {
#if UNITY_EDITOR
            if (defaultCamera == null)
                Debug.LogError("CameraMovement ma jai ne defaultCamera to app bhai !!!");
#endif
            isMouseDown = false;
            CanMoveCamera = true;
            StopInteraction = false;
        }

        private void Update()
        {
            //#if PLATFORM_ANDROID
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HideLastOpenView(true);
            }
            //#endif
            if (StopInteraction)
            {
                return;
            }

            if (EventSystem.current != null && (EventSystem.current.currentSelectedGameObject != null || RaycastBlock))
            {
                InvokeOnUIClick(Input.mousePosition);
#if UNITY_EDITOR
                if (Input.GetMouseButtonUp(0))
                {
                    InvokeOnUIPointerUp(Input.mousePosition);
                }
#endif

#if !UNITY_EDITOR
                if (Input.touchCount > 0)
                {
                    touch = Input.GetTouch(0);
                    if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
                    {
                        InvokeOnUIPointerUp(touch.position);
                    }
                }
#endif
                return;
            }

#if UNITY_EDITOR
            EditorInput();
#endif
#if !UNITY_EDITOR
            MobileInput();
#endif
        }


        private void MobileInput()
        {
            if (Input.touchCount > 0)
            {
                touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    InvakeMouseButtonDown(touch.position);
                    isMouseDown = true;
                }
                else if (isMouseDown)
                {
                    InvokeMouseMove(touch.position);
                }

                if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
                {
                    InvakeMouseButtonUp(touch.position);
                    isMouseDown = false;
                }
            }

            if (!CanMoveCamera && Input.touchCount <= 0)
            {
                CanMoveCamera = true;
            }
        }

        public void HideLastOpenView(bool isShowQuitView = false)
        {
            if (BaseView.openView.Count > 0)
            {
                BaseView lastOpenView = BaseView.openView[BaseView.openView.Count - 1];
                if (lastOpenView.AllowBackButton)
                    lastOpenView.Hide();
            }
        }

        private void QuitApplication()
        {
            Debug.Log("QuitApplication");
            LocalPushNotificationManager.Instance.ScheduleNotification(true);
            Application.Quit();
        }

        private void EditorInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                InvakeMouseButtonDown(Input.mousePosition);
                isMouseDown = true;
            }
            else if (isMouseDown)
            {
                InvokeMouseMove(Input.mousePosition);
            }

            if (Input.GetMouseButtonUp(0))
            {
                InvakeMouseButtonUp(Input.mousePosition);
                isMouseDown = false;
            }
        }
        #endregion

        #region public methods

        public void AddListenerMouseButtonDown(Action<Vector3> action)
        {
            if (!mouseButtonDownEvent.Contains(action))
                mouseButtonDownEvent.Add(action);
        }

        public void RemoveListenerMouseButtonDown(Action<Vector3> action)
        {
            if (mouseButtonDownEvent.Contains(action))
                mouseButtonDownEvent.Remove(action);
        }

        public void AddListenerMouseButtonUp(Action<Vector3> action)
        {
            if (!mouseButtonUpEvent.Contains(action))
                mouseButtonUpEvent.Add(action);
        }

        public void RemoveListenerMouseButtonUp(Action<Vector3> action)
        {
            if (mouseButtonUpEvent.Contains(action))
                mouseButtonUpEvent.Remove(action);
        }

        public void AddListenerMouseButtonMove(Action<Vector3> action)
        {
            if (!mouseButtonMoveEvent.Contains(action))
                mouseButtonMoveEvent.Add(action);
        }

        public void RemoveListenerMouseButtonMove(Action<Vector3> action)
        {
            if (mouseButtonMoveEvent.Contains(action))
                mouseButtonMoveEvent.Remove(action);
        }

        public void AddListenerUIClick(Action<Vector3> action)
        {
            if (!onUIClick.Contains(action))
                onUIClick.Add(action);
        }

        public void RemoveListenerUIClick(Action<Vector3> action)
        {
            if (onUIClick.Contains(action))
                onUIClick.Remove(action);
        }

        public void AddListenerUIPointerUp(Action<Vector3> action)
        {
            if (!onUIPointerUp.Contains(action))
                onUIPointerUp.Add(action);
        }

        public void RemoveListenerUIPointerUp(Action<Vector3> action)
        {
            if (onUIPointerUp.Contains(action))
                onUIPointerUp.Remove(action);
        }

        #endregion

        #region private methods

        private void InvakeMouseButtonDown(Vector3 pos)
        {
            if (convertToWorldSpace)
                pos = EventCamera.ScreenToWorldPoint(pos);
            foreach (var ev in mouseButtonDownEvent)
            {
                ev?.Invoke(pos);
            }
        }

        private void InvakeMouseButtonUp(Vector3 pos)
        {
            if (convertToWorldSpace)
                pos = EventCamera.ScreenToWorldPoint(pos);
            foreach (var ev in mouseButtonUpEvent)
            {
                ev?.Invoke(pos);
            }
        }

        private void InvokeMouseMove(Vector3 pos)
        {
            if (convertToWorldSpace)
                pos = EventCamera.ScreenToWorldPoint(pos);

            foreach (var ev in mouseButtonMoveEvent)
            {
                ev?.Invoke(pos);
            }
        }

        private void InvokeOnUIClick(Vector3 pos)
        {
            if (convertToWorldSpace)
                pos = EventCamera.ScreenToWorldPoint(pos);
            foreach (var ev in onUIClick)
            {
                ev?.Invoke(pos);
            }
        }

        private void InvokeOnUIPointerUp(Vector3 pos)
        {
            if (convertToWorldSpace)
                pos = EventCamera.ScreenToWorldPoint(pos);
            foreach (var ev in onUIPointerUp)
            {
                ev?.Invoke(pos);
            }
        }
        #endregion
    }
}