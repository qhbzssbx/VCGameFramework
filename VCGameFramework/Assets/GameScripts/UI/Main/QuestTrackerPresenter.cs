using VContainer.Unity;

namespace Game.UI.Main
{
    public class QuestTrackerPresenter : IStartable
    {
        private readonly QuestTrackerView _view;
        public QuestTrackerPresenter(QuestTrackerView view) => _view = view;
        public void Start() {}
    }
}
