using DonutStack.Core.MVP.Presenter;

namespace DonutStack.Common.UI.LevelScore
{
    public class ScorePresenter : BasePresenter<ScoreView, ScoreModel>
    {
        public ScorePresenter(ScoreView view) : base(view) { }

        public void SetModel(ScoreModel model) => Model = model;

        public void CallOnQuit() => Model.OnQuit();

        public void CallUpdateScore(int score, int target, int oldScore) => View.UpdateScore(score, target, oldScore);

        public DialogManager GetDialogManager() => View.GetDialogManager();
    }
}
