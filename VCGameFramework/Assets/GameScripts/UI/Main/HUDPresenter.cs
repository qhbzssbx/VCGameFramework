using VContainer.Unity;

namespace Game.UI.Main
{
    /// <summary>
    /// HUD 显示逻辑
    /// </summary>
    public class HUDPresenter : IStartable
    {
        private readonly HUDView _view;

        public HUDPresenter(HUDView view) => _view = view;

        /// <inheritdoc />
        public void Start() {}
    }
}
