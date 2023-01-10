using DonutStack.Common.Stack;
using DonutStack.Core.MVP.View;
using DonutStack.Core.Pooling;
using UnityEngine;

namespace DonutStack.Common.Board
{
    public class BoardView : BaseView<BoardPresenter>
    {
        [SerializeField] private MeshRenderer[] rowsRenderers;
        [SerializeField] private Transform stacksContainer;
        [SerializeField] private int collumns;

        public override void OnInit() { }

        private void Awake()
        {
            Presenter = new BoardPresenter(this);
            BoardModel model = new BoardModel(Presenter);
            Presenter.SetModel(model);

            foreach (var renderer in rowsRenderers)
            {
                renderer.enabled = false;
            }
            Presenter.SetRows(rowsRenderers.Length);
            Presenter.SetCollumns(collumns);
        }

        public void SetCurrentRow(int currentRow)
        {
            for (int i = 0; i < rowsRenderers.Length; i++)
            {
                rowsRenderers[i].enabled = currentRow == i;
            }
        }

        public StackView SpawnStack(string key, Vector3 pos) 
        {
            return ObjectPooler.Instance.SpawnFromPool(key, pos, stacksContainer).GetTransform().gameObject.GetComponent<StackView>();
        }

        private void OnApplicationQuit()
        {
            Presenter.OnQuit();
        }


    }
}
