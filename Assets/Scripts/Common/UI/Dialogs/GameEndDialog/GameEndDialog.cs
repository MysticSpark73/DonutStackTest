using DonutStack.Common.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DonutStack.Common.UI.Dialogs.GameEndDialog
{
    public class GameEndDialog : Dialog
    {
        [Header("GameEndDialog")]
        [SerializeField] Button restartButton;
        [SerializeField] TextMeshProUGUI header;

        protected override void Init(string[] args)
        {
            base.Init(args);
            if (args != null)
            {
                header.text = args[0];
            }
        }

        protected override void Onshow()
        {
            base.Onshow();
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(Restart);
        }

        private void Restart() 
        {
            EventManager.OnGameRestart?.Invoke();
            Hide();
        }
    }
}
