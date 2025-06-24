using UnityEngine;

namespace GameScript.Core.UI.Core
{
    /// <summary>
    /// 所有 UI 逻辑类的基类
    /// </summary>
    public abstract class UILogic : MonoBehaviour
    {
        public virtual void OnOpen(params object[] args) { }
        public virtual void OnClose() => Destroy(gameObject);
    }
}
