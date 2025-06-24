using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using GameScript.Core.UI.Core;

public class UILoadingPanel : UILogic
{
    public Image backgroundImage;
    public TextMeshProUGUI tipText;
    public Slider progressSlider;

    public List<Sprite> backgroundSprites;
    public List<string> tips;
    public float switchInterval = 2f;

    private float _timer = 0f;
    private int _index = 0;
    private bool _isRunning = false;
    private AsyncOperation _operation;

    public override void OnOpen(params object[] args)
    {
        if (args.Length > 0 && args[0] is AsyncOperation op)
        {
            _operation = op;
            _isRunning = true;
            StartLoop().Forget();
        }
        else
        {
            Debug.LogError("UILoadingPanel: 缺少 AsyncOperation 参数");
        }
    }

    private async UniTaskVoid StartLoop()
    {
        while (_isRunning && _operation != null && !_operation.isDone)
        {
            _timer += Time.deltaTime;
            if (_timer >= switchInterval)
            {
                _timer = 0;
                _index = (_index + 1) % backgroundSprites.Count;
                backgroundImage.sprite = backgroundSprites[_index];
                tipText.text = tips[_index % tips.Count];
            }

            float progress = Mathf.Clamp01(_operation.progress / 0.9f);
            progressSlider.value = progress;

            await UniTask.Yield();
        }

        _isRunning = false;
        UIManager.Instance.CloseUI<UILoadingPanel>();
    }
}
