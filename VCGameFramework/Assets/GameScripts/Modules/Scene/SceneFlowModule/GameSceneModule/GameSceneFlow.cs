using Cysharp.Threading.Tasks;

public class GameSceneFlow : ISceneFlow
{
    private readonly StorySystem story;

    public GameSceneFlow(StorySystem story)
    {
        this.story = story;
    }

    public async UniTask OnEnter(object args)
    {
        if (args is string from && from == "Login")
        {
            await story.PlayIntro();
        }
    }

    public async UniTask OnExit()
    {
    }
}
