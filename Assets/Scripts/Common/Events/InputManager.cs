using UnityEngine;

namespace DonutStack.Common.Events
{
    public class InputManager : MonoBehaviour
    {
        private static bool isTouchLocked = false;
        private TouchState touchState;
        private Vector2 touchPos;
        private enum TouchState : byte { NoTouch, TouchDown }

        private void Update()
        {
            CheckTouchState();
        }

        public static void LockTouch() => isTouchLocked = true;
        public static void UnlockTouch() => isTouchLocked = false;

        private void CheckTouchState()
        {
            if (isTouchLocked)
            {
                if (touchState != TouchState.NoTouch)
                {
                    EventManager.OnTouchUp?.Invoke();
                }
                touchState = TouchState.NoTouch;
                return;
            }

#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0)) 
            {
                touchState = TouchState.TouchDown;
                touchPos = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (touchState != TouchState.TouchDown)
                {
                    return;
                }
                EventManager.OnTouchUp?.Invoke();
                touchState = TouchState.NoTouch;
            }

            if (touchState == TouchState.TouchDown)
            {
                touchPos = Input.mousePosition;
                EventManager.OnTouchDown?.Invoke(touchPos);
            }

#else

            if (Input.touchCount > 0)
            {
                touchState = TouchState.TouchDown;
                touchPos = Input.GetTouch(0).position;
            }

            if (Input.touchCount == 0)
            {
                if (touchState != TouchState.TouchDown)
                {
                    return;
                }
                EventManager.OnTouchUp?.Invoke();
                touchState = TouchState.NoTouch;
            }

            if (touchState == TouchState.TouchDown)
            {
                touchPos = Input.GetTouch(0).position;
                EventManager.OnTouchDown?.Invoke(touchPos);
            }
#endif
        }

    }
}
