using DG.Tweening;
using DonutStack.Common.Events;
using DonutStack.Core.MVP.View;
using DonutStack.Core.Pooling;
using DonutStack.Data.Parameters;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace DonutStack.Common.Stack
{
    public class StackView : BaseView<StackPresenter>, IPoolable
    {
        public bool IsMoving { get; private set; }
        public bool IsActive => gameObject.activeSelf;

        public Vector2Int gridPos;
        //Stacked objects arranged from 0 to 2, bottom to up
        [SerializeField] private Transform[] stackedAnchors;
        [SerializeField] private Transform pillarTransform;
        [SerializeField] private MeshRenderer pillarRenderer;
        [SerializeField] private GameObject particleEffect;
        [SerializeField] private Rigidbody rigidbody;

        private Vector3 pillarBaseScale = new Vector3(.2f, .5f, .2f);
        private Vector3 stopAnimScale = new Vector3(1.15f, 1, .85f);
        private bool IsComplete = false;

        //animations
        private float throwDuration = .3f;
        private float disappearDuration = .2f;
        private float stopAnimDuration = .1f;

        public override void OnInit() { }

        private void Awake()
        {
            IStackedObject[] stackedObjects = new IStackedObject[3];
            for (int i = 0; i < stackedAnchors.Length; i++)
            {
                stackedObjects[i] = stackedAnchors[i].GetChild(0).GetComponent<IStackedObject>();
            }
            Presenter = new StackPresenter(this);
            StackModel model = new StackModel(Presenter, stackedObjects);
            Presenter.SetModel(model);
        }

        private void Update()
        {
            CheckMoving();
        }

        private void CheckMoving()
        {
            if (IsMoving && rigidbody.velocity.z <= 1)
            {
                IsMoving = false;
                rigidbody.isKinematic = true;
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
                EventManager.OnStackStop?.Invoke(this);
            }
        }

        #region InterfaceImplementation

        public void OnPool()
        {
            rigidbody.isKinematic = false;
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            pillarTransform.localScale = pillarBaseScale;
            pillarRenderer.materials[0].color = Parameters.donut_pillar_color;
            Presenter.CallGenerateRandomObjects();
        }

        public void OnReturn()
        {
            DisappearAnim(() => ObjectPooler.Instance.ReturnIntoPool(Parameters.object_pooler_key_stack, gameObject));
        }

        public Transform GetTransform() => transform;

        #endregion

        public void SetMoving() => IsMoving = true;

        public void SetComplete() => IsComplete = true;

        public int GetActiveObjectsCount() => Presenter.GetActiveObjectsCount();

        public bool GetIsReturning() => Presenter.GetIsReturning();

        public bool GetComplete() => IsComplete;

        public async Task AddObject(StackedObjectColor color, bool animate = true) => await Presenter.CallAddObject(color, animate);

        public async Task RemoveObject(bool animate = true) => await Presenter.CallRemoveObject(animate);

        public Rigidbody GetRigidbody() => rigidbody;

        public Transform GetFirstEmptyAnchor() => stackedAnchors[GetActiveObjectsCount()];

        public StackedObjectColor GetTopObjectColor() => Presenter.CallGetTopObjectColor();

        public async Task ThrowAnim(StackView target, Action callback = null) {
            Transform targetAnchor = target.GetFirstEmptyAnchor();
            Transform objectTransform = Presenter.CallGetTopObject().GetTransform();
            Vector3 direction = (targetAnchor.position - objectTransform.position).normalized;
            StackedObjectColor topColor = Presenter.CallGetTopObjectColor();

            (Vector3, Vector2) GetRotationAxis(Vector3 dir) {
                float threshold = .1f;
                float dot = Vector2.Dot(new Vector2 (dir.x, dir.z).normalized, Vector2.right);
                if (dot <= 1 && dot >= 1 - threshold)
                {
                    return (Vector3.right, Vector2.right);
                }
                else if (dot >= -1 && dot <= -1 + threshold)
                {
                    return (Vector3.left, Vector2.right);
                }
                dot = Vector2.Dot(new Vector2(dir.x, dir.z).normalized, Vector2.up);
                if (dot <= 1 && dot >= 1 - threshold)
                {
                    return (Vector3.right, Vector2.up);
                }
                else if (dot >= -1 && dot <= -1 + threshold)
                {
                    return (Vector3.left, Vector2.up);
                }
                return (Vector3.zero, Vector2.zero);
            }

            var (rot, dot) = GetRotationAxis(direction);


            //animation itself
            bool isComplete = false;
            if (rot == Vector3.zero)
            {
                Debug.LogError($"[StackView][ThrowObject] {this} Rotation vector is empty! Can't resolve the animation!");
                isComplete = true;
            }
            if (dot == Vector2.up)
            {
                objectTransform.localRotation = Quaternion.Euler(Vector3.right * -90);
            }
            if (dot == Vector2.right)
            {
                objectTransform.localRotation = Quaternion.Euler(Vector3.right * -90 + Vector3.up * 90);
            }

            DOTween.Kill(this);
            Sequence sequence = DOTween.Sequence();
            var moveX = objectTransform.DOMoveX(targetAnchor.position.x, throwDuration).SetEase(Ease.Linear);
            var moveYUp = objectTransform.DOMoveY((objectTransform.position.y + 1) * 1.25f, throwDuration * .5f).SetEase(Ease.OutCubic);
            var moveYDown = objectTransform.DOMoveY(targetAnchor.position.y, throwDuration * .5f).SetEase(Ease.InCubic);
            var moveZ = objectTransform.DOMoveZ(targetAnchor.position.z, throwDuration).SetEase(Ease.Linear);
            var rotation = objectTransform.DOLocalRotate(rot * 180, throwDuration, RotateMode.FastBeyond360).SetRelative().SetEase(Ease.Linear);
            sequence.Insert(0, moveX);
            sequence.Insert(0, moveYUp);
            sequence.Insert(0, moveZ);
            sequence.Insert(throwDuration * .5f, moveYDown);
            sequence.Insert(0, rotation);
            sequence.OnComplete(() => isComplete = true);

            sequence.Play();

            await new WaitUntil(() => isComplete);
            await RemoveObject(false);
            await target.AddObject(topColor, true);
            callback?.Invoke();
        }

        public async Task StopAnim(Action callback = null) 
        {
            DOTween.Kill(this);
            bool isComplete = false;
            Sequence sequence = DOTween.Sequence();
            sequence.Insert(0, transform.DOScale(stopAnimScale, stopAnimDuration).From(Vector3.one).SetEase(Ease.OutSine));
            sequence.Insert(stopAnimDuration, transform.DOScale(Vector3.one, stopAnimDuration).From(stopAnimScale).SetEase(Ease.InSine));
            sequence.OnComplete(() => isComplete = true);
            sequence.Play();
            await new WaitUntil(() => isComplete);
            callback?.Invoke();
        }

        private async void DisappearAnim(Action callback = null) {
            bool isComplete = false;
            await Presenter.CallDeactivateAllObjects();
            particleEffect.SetActive(true);
            pillarTransform.DOScale(Vector3.zero, disappearDuration).SetEase(Ease.InBack)
                .OnComplete(() => {
                    isComplete = true;
                });
            await new WaitUntil(() => isComplete);
            await new WaitForSeconds(.7f);
            particleEffect.SetActive(false);
            EventManager.OnStackRemoved?.Invoke(this);
            callback?.Invoke();
        }

    }
}
