using DG.Tweening;
using DonutStack.Core.MVP.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DonutStack.Common.UI.LevelScore
{
    public class ScoreView : BaseView<ScorePresenter>
    {
        [SerializeField] private DialogManager dialogManager;
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Image fillImage;

        private float scoreAnimDuration = .25f;

        private void Awake()
        {
            Presenter = new ScorePresenter(this);
            ScoreModel model = new ScoreModel(Presenter);
            Presenter.SetModel(model);
        }

        public override void OnInit() { }

        public DialogManager GetDialogManager() => dialogManager;

        public void UpdateScore(int score, int target, int oldScore)
        {
            AnimateScore(score, target, oldScore);
        }

        private async void AnimateScore(int score, int target, int oldScore) 
        {
            float old = oldScore;
            float updated = score;
            float aim = target;
            bool isComplete = false;
            DOTween.To(() => old, x => old = x, updated, scoreAnimDuration)
                .OnUpdate(
                () => {
                    //animate text
                    scoreText.text = ((int)old).ToString();
                    //animate Slider
                    slider.value = (float)((float)old / (float)aim);
                    //animate color
                    fillImage.color = new Color(
                        1.0f - Mathf.Min( Mathf.Max((float) old - (float) aim * 0.5f, 0) / ((float) aim * 0.5f), 1),
                        Mathf.Min((float) old / ((float) aim *0.5f), 1), 
                        0);
                    }
                ).OnComplete(() => isComplete = true );
            await new WaitUntil(() => isComplete);
        }

        private void OnApplicationQuit()
        {
            Presenter.CallOnQuit();
        }

    }
}
