using VContainer.Unity;

namespace Game.UI.Main
{
    public class MinimapPresenter : IStartable
    {
        private readonly MinimapView _view;
        public MinimapPresenter(MinimapView view) => _view = view;
        public void Start() {}
    }
}
