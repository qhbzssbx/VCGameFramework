using VContainer.Unity;

namespace Game.UI.Main
{
    /// <summary>
    /// 小地图显示逻辑
    /// </summary>
    public class MinimapPresenter : IStartable
    {
        private readonly MinimapView _view;

        public MinimapPresenter(MinimapView view) => _view = view;

        /// <inheritdoc />
        public void Start() {}
    }
}
