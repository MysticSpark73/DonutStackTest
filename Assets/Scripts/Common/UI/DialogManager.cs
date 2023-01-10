using DonutStack.Common.Events;
using DonutStack.Common.UI.Dialogs;
using DonutStack.Common.UI.Dialogs.GameEndDialog;
using DonutStack.Data;
using DonutStack.Data.Parameters;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DonutStack.Common.UI
{
    public class DialogManager : MonoBehaviour
    {
        [SerializeField] private List<Dialog> dialogs;

        private void Awake()
        {
            EventManager.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnApplicationQuit()
        {
            EventManager.OnGameStateChanged -= OnGameStateChanged;
        }

        public void ShowDialog<T>(string[] args = null) 
        {
            var dialog = dialogs.FirstOrDefault(d => d is T);
            if (dialog == null)
            {
                Debug.LogError($"Cannot find a dialog of type {typeof(T)}");
            }
            dialog.Show(true, args);
        }

        private void OnGameStateChanged(GameState gameState) 
        {
            if (gameState == GameState.GameEnd)
            {
                if (Parameters.score < Parameters.targetScore)
                {
                    ShowDialog<GameEndDialog>(new string[] { Parameters.game_end_dialog_lost_header});
                }
                else
                {
                    ShowDialog<GameEndDialog>(new string[] {Parameters.game_end_dialog_won_header});
                }
            }
        }


    }
}