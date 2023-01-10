using DonutStack.Core.MVP.View;

namespace DonutStack.Core.MVP.Presenter
{
    public class BasePresenter<TV, TM> : IPresenter
        where TV : IView
        where TM : IModel
    {
        public TV View { get; set; }
        public TM Model { get; set; }

        public BasePresenter(TV view)
        {
            View = view;
            View.OnInit();
        }
    }

}
