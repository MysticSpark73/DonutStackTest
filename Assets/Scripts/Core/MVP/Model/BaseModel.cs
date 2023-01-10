using DonutStack.Core.MVP.Presenter;

namespace DonutStack.Core.MVP.Model
{
    public class BaseModel<TP> : IModel
        where TP : IPresenter
    {
        public TP Presenter { get ; set; }

        public BaseModel(TP presenter) {
            Presenter = presenter;
        }

    }
}
