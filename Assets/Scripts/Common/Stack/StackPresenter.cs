using DonutStack.Core.MVP.Presenter;
using System.Threading.Tasks;
using UnityEngine;

namespace DonutStack.Common.Stack
{
    public class StackPresenter : BasePresenter<StackView, StackModel>
    {
        public StackPresenter(StackView view) : base(view) { }

        public void SetModel(StackModel model) => Model = model;
        
        public void CallGenerateRandomObjects() => Model.GenerateRandomObjects();

        public void CallOnReturn() => View.OnReturn();

        public void SetComplete() => View.SetComplete();

        public int GetActiveObjectsCount() => Model.ActiveObjects;

        public bool GetIsReturning() => Model.GetIsReturning();

        public async Task CallAddObject(StackedObjectColor color, bool animate = true) => await Model.AddObject(color, animate);

        public async Task CallRemoveObject(bool animate = true) => await Model.RemoveObject(animate);

        public async Task CallDeactivateAllObjects() => await Model.DeactivateAllObjects();

        public StackedObjectColor CallGetTopObjectColor() => Model.GetTopObjectColor();

        public IStackedObject CallGetTopObject() => Model.GetTopObject();

    }
}
