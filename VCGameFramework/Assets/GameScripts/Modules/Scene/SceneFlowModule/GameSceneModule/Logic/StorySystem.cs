using Cysharp.Threading.Tasks;

public class StorySystem
{
    public async UniTask PlayIntro()
    {
        UnityEngine.Debug.Log("播放 GameScene 剧情");
        await UniTask.Delay(1000);
    }
}
