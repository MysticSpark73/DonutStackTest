using DG.Tweening;
using UnityEngine;

namespace DonutStack.Common.UI.Dialogs
{
    public class Dialog : MonoBehaviour
    {
        [Header("Dialog")]
        [SerializeField] private Transform rootTransform;

        private float showAnimDuration = .5f;
        private float hideAnimDuration = .25f;

        public async void Show(bool animate = true, string[] args = null) 
        {
            Init(args);
            rootTransform.localScale = Vector3.zero;
            rootTransform.gameObject.SetActive(true);
            bool isComplete = false;
            rootTransform.DOScale(Vector3.one, animate ? showAnimDuration: 0).From(Vector3.zero).SetEase(Ease.OutBack)
                .OnComplete(() => isComplete = true);
            await new WaitUntil(() => isComplete);
            Onshow();
        }

        public async void Hide(bool animate = true) 
        {
            bool isComplete = false;
            rootTransform.DOScale(Vector3.zero, animate ? hideAnimDuration : 0).From(Vector3.one).SetEase(Ease.OutBack)
                .OnComplete(() => isComplete = true);
            await new WaitUntil(() => isComplete);
        }

        protected virtual void Onshow() { }

        protected virtual void Init(string[] args) { }
    }
}
