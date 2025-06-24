using DG.Tweening;
using MessagePipe;
using TMPro;
using UnityEngine;
using VContainer;

public class LaunchAnim : MonoBehaviour
{

    [Inject]
    IPublisher<int> publisher;

    public TMP_Text tMP;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(">>>>>>>>>>>>>>");
        tMP.DOText("GameStart", 0.2f).SetRelative().SetEase(Ease.Linear).SetAutoKill(false).OnComplete(() => {
            publisher.Publish(1002);
        });
    }
    // Update is called once per frame
    void Update()
    {
        
    }

}
