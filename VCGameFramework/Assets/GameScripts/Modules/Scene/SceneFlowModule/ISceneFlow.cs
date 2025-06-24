using Cysharp.Threading.Tasks;

public interface ISceneFlow
{
    UniTask OnEnter(object args);
    UniTask OnExit();
}
