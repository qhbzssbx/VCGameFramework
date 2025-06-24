using Cysharp.Threading.Tasks;

public class GameSceneFlow : ISceneFlow
{
    private readonly MainUI mainUI;
    private readonly StorySystem story;

    public GameSceneFlow(MainUI mainUI, StorySystem story)
    {
        this.mainUI = mainUI;
        this.story = story;
    }

    public async UniTask OnEnter(object args)
    {
        mainUI.Show();
        if (args is string from && from == "Login")
        {
            await story.PlayIntro();
        }
    }

    public async UniTask OnExit()
    {
        mainUI.Hide();
    }
}
