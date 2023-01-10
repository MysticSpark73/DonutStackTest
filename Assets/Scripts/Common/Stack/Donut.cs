using DG.Tweening;
using DonutStack.Common.Audio;
using DonutStack.Data.Parameters;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace DonutStack.Common.Stack
{
    public class Donut : MonoBehaviour, IStackedObject
    {
        [SerializeField] private Transform selfTransform;
        [SerializeField] private MeshRenderer meshRenderer;

        [SerializeField] private StackedObjectColor color;

        //animations
        private float activateAnimDuration = .5f;
        private float deactivateAnimDuration = .25f;

        public bool IsActive { get; set; }

        #region InterfaceMethods

        public void Init(StackedObjectColor color, Action callback = null)
        {
            SetColor(color);
            callback?.Invoke();
        }

        public Transform GetTransform() => selfTransform;

        public StackedObjectColor GetColor() => color;

        public void SetColor(StackedObjectColor color)
        {
            this.color = color;
            SetMaterialColor();
        }

        public async Task Activate(bool animate = true)
        {
            IsActive = true;
            gameObject.SetActive(true);
            if (animate)
            {
                AudioController.Instance.PlaySound(AudioController.Sounds.DonutFall);
            }
            await ActivateAnim(animate);
        }

        public async Task Deactivate(bool animate = true)
        {
            if (animate)
            {
                AudioController.Instance.PlaySound(AudioController.Sounds.DonutPop);
            }
            await DeactivateAnim(animate);
            IsActive = false;
            gameObject.SetActive(false);
        }

        #endregion

        private void SetMaterialColor()
        {
            Color col = Parameters.StackedColorToColor(color);
            if (col == Color.clear)
            {
                Debug.LogError($"[Donut][Init] Color {color} not found!");
                return;
            }
            meshRenderer.materials[0].color = col;

        }

        private async Task ActivateAnim(bool animate = true, Action callback = null)
        {
            DOTween.Kill(this);
            bool isComplete = false;
            transform.rotation = Quaternion.Euler(Vector3.right * -90);
            if (animate)
            {
                selfTransform.DOScale(Vector3.one, activateAnimDuration).From(Vector3.zero)
                    .SetEase(Ease.OutBack).OnComplete(() => isComplete = true);
            }
            else
            {
                selfTransform.localScale = Vector3.one;
                isComplete = true;
            }
            await new WaitUntil(() => isComplete);
            callback?.Invoke();
        }

        private async Task DeactivateAnim(bool animate = true, Action callback = null)
        {
            DOTween.Kill(this);
            bool isComplete = false;
            if (animate)
            {
                selfTransform.DOScale(Vector3.zero, deactivateAnimDuration).From(Vector3.one)
                    .SetEase(Ease.InBack).OnComplete(() => isComplete = true);
            }
            else
            {
                selfTransform.localScale = Vector3.zero;
                isComplete = true;
            }
            selfTransform.localPosition = Vector3.zero;
            await new WaitUntil(() => isComplete);
            callback?.Invoke();
        }

    }
}
