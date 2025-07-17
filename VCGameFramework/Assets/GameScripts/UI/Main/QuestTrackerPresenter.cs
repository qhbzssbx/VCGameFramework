using VContainer.Unity;

namespace Game.UI.Main
{
    /// <summary>
    /// 任务追踪显示逻辑
    /// </summary>
    public class QuestTrackerPresenter : IStartable
    {
        private readonly QuestTrackerView _view;

        public QuestTrackerPresenter(QuestTrackerView view) => _view = view;

        /// <inheritdoc />
        public void Start() {}
    }
}
