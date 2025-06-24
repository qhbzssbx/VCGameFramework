using GameScripts.Modules.Scene.Runtime;
using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class LaunchScript : IStartable
{
    CustomSceneManager sceneManager;
    ISubscriber<int> subscriber;

    public LaunchScript(CustomSceneManager customSceneManager, ISubscriber<int> subscriber)
    {
        sceneManager = customSceneManager;
        this.subscriber = subscriber;

        // 订阅消息
        this.subscriber.Subscribe(EventHandler).AddTo(DisposableBag.CreateBuilder());
    }

    public void Start()
    {
        Debug.Log("Game Start");
        
    }

    private void EventHandler(int eventId)
    {
        if (eventId == 1002)
        {
            SwitchScene();
        }
    }

    private void SwitchScene()
    {
        Debug.Log("LaunchAnimEnd -> SwitchScene");
        sceneManager.LoadSceneAsync("HotUpdate").Forget();
    }
}
