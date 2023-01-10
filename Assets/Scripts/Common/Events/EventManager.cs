using DonutStack.Common.Stack;
using DonutStack.Data;
using System;
using UnityEngine;

namespace DonutStack.Common.Events
{
    public static class EventManager
    {
        #region TouchInput

        public static Action<Vector2> OnTouchDown;
        public static Action OnTouchUp;

        #endregion
        #region Stack

        public static Action<StackView> OnStackStop;
        public static Action<StackView> OnStackRemoved;

        #endregion
        #region Score

        public static Action OnScoreChanged;

        #endregion
        #region GameState

        public static Action<GameState> OnGameStateChanged;
        public static Action OnGameRestart;

        #endregion


    }
}