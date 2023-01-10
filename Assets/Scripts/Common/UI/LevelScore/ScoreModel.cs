using DonutStack.Common.Audio;
using DonutStack.Common.Events;
using DonutStack.Core.MVP.Model;
using DonutStack.Data.Parameters;
using UnityEngine;

namespace DonutStack.Common.UI.LevelScore
{
    public class ScoreModel : BaseModel<ScorePresenter>
    {
        private DialogManager dialogManager;
        private int score, oldScore;

        public ScoreModel(ScorePresenter presenter) : base(presenter)
        {
            EventManager.OnScoreChanged += OnScoreChanged;
            dialogManager = Presenter.GetDialogManager();
        }

        public void OnScoreChanged() 
        {
            oldScore = score;
            score = Mathf.Min(Parameters.score, Parameters.targetScore);
            Presenter.CallUpdateScore(score, Parameters.targetScore, oldScore);
            if (score == Parameters.targetScore)
            {
                Parameters.SetGameState(Data.GameState.GameEnd);
                AudioController.Instance.PlaySound(AudioController.Sounds.WinSound);
            }
        }

        public void OnQuit() 
        {
            EventManager.OnScoreChanged -= OnScoreChanged;
        }

    }
}
