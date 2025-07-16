using VContainer.Unity;

namespace Game.UI.Main
{
    public class HUDPresenter : IStartable
    {
        private readonly HUDView _view;
        public HUDPresenter(HUDView view) => _view = view;
        public void Start() {}
    }
}
