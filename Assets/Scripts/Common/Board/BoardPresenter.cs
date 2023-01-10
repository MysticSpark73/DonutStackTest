using DonutStack.Common.Stack;
using DonutStack.Core.MVP.Presenter;
using UnityEngine;

namespace DonutStack.Common.Board
{
    public class BoardPresenter : BasePresenter<BoardView, BoardModel>
    {
        public BoardPresenter(BoardView view) : base(view) { }

        public void SetModel(BoardModel model) => Model = model;

        public void SetRows(int rows) => Model.RowsNumber = rows;

        public void SetCollumns(int collumns) => Model.CollumnsNumber = collumns;

        public void SetCurrentRow() => View.SetCurrentRow(Model.CurrentRow);

        public void OnQuit() => Model.OnQuit();

        public StackView SpawnStack(string key, Vector3 pos) => View.SpawnStack(key, pos);

    }
}
