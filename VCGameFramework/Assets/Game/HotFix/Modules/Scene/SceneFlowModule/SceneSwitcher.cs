using UnityEngine.SceneManagement;
using VContainer.Unity;
using Cysharp.Threading.Tasks;
using VContainer;

public class SceneSwitcher
{
    private LifetimeScope currentScope;

    //public async UniTask SwitchScene(string sceneName, object param = null)
    //{
    //    if (currentScope != null)
    //    {
    //        var oldFlow = currentScope.Container.Resolve<ISceneFlow>();
    //        await oldFlow.OnExit();
    //        currentScope.Dispose();
    //    }

    //    var handle = SceneManager.LoadSceneAsync(sceneName);
    //    handle.allowSceneActivation = true;
    //    while (!handle.isDone)
    //        await UniTask.Yield();

    //    currentScope = LifetimeScope.Find<SceneLifetimeScope>();
    //    var newFlow = currentScope.Container.Resolve<ISceneFlow>();
    //    await newFlow.OnEnter(param);
    //}
}
