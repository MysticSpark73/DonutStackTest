using DonutStack.Core.MVP.Presenter;
using UnityEngine;

namespace DonutStack.Core.MVP.View
{
    public class BaseView<TP> : MonoBehaviour, IView
        where TP : IPresenter
    {
        public TP Presenter { get; set; }

        public virtual void OnInit(){ }

    }
}
