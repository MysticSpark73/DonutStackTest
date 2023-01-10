using DonutStack.Common.Events;
using DonutStack.Common.Stack;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DonutStack.Data.Parameters
{
    public static class Parameters
    {
        #region DonutColors

        public static readonly Color donut_color_blue = new Color(0, 0.6f, 1, 1);
        public static readonly Color donut_color_brown = new Color(0.3f, 0.15f, 0, 1);
        public static readonly Color donut_color_pink = new Color(1, 0, 1, 1);
        public static readonly Color donut_color_yellow = new Color(1, 0.7601946f, 0, 1);
        public static readonly Color donut_pillar_color = new Color(0.95f, 0.8f, 0.7f, 1);

        public static Color StackedColorToColor(StackedObjectColor color)
        {
            switch (color)
            {
                case StackedObjectColor.Blue:
                    return donut_color_blue;
                case StackedObjectColor.Brown:
                    return donut_color_brown;
                case StackedObjectColor.Pink:
                    return donut_color_pink;
                case StackedObjectColor.Yellow:
                    return donut_color_yellow;
                default:
                    return Color.clear;
            }
        }

        public static StackedObjectColor GetRandomStackedColor() {
            var colors = Enum.GetValues(typeof(StackedObjectColor));
            return (StackedObjectColor) colors.GetValue(UnityEngine.Random.Range(0, colors.Length));
        }

        public static StackedObjectColor GetRandomStackedColor(StackedObjectColor excludeColor) {
            var colors = (StackedObjectColor[]) Enum.GetValues(typeof(StackedObjectColor));
            List<StackedObjectColor> colorsList = new List<StackedObjectColor>(colors);
            colorsList.Remove(excludeColor);
            StackedObjectColor res = colorsList[UnityEngine.Random.Range(0, colorsList.Count)];
            if (excludeColor == res)
            {
                Debug.Log($"THE RETURNED COLOR : {res} EQUALS TO THE EXCLUDED COLOR {excludeColor}");
            }
            return res;
        }

        #endregion

        #region Score

        public static int score { get; private set; }

        public static int targetScore = 1000;

        public static void AddScore(int value)
        { 
            score += value;
            EventManager.OnScoreChanged?.Invoke();
        }

        public static void SubtractScore(int value)
        {
            score = Mathf.Max(score - value, 0);
            EventManager.OnScoreChanged?.Invoke();
        }

        public static void ResetScore()
        {
            score = 0;
            EventManager.OnScoreChanged?.Invoke();
        }

        #endregion

        #region GameState

        public static GameState gameState;

        public static void SetGameState(GameState state)
        {
            gameState = state;
            EventManager.OnGameStateChanged?.Invoke(gameState);
        }

        #endregion

        #region ObjectPoolerKeys

        public static readonly string object_pooler_key_stack = "Stack";

        #endregion

        #region GameEndDialog

        public static readonly string game_end_dialog_won_header = "Congratulations!!!";
        public static readonly string game_end_dialog_lost_header = "You Lose. Try again!";

        #endregion
    }
}
