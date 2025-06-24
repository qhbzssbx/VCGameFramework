using UnityEngine;
using VContainer;
using VContainer.Unity;

public class Test2 : IStartable
{
    [Inject]
    Test test;

    void IStartable.Start()
    {
        Debug.Log("!!!!!  " + test.test);
    }

}
