// 放到任意 Editor/Runtime Assembly 都行
namespace System.Runtime.CompilerServices
{
    // 兼容 C# 9 init-only setter 的必要类型
    internal static class IsExternalInit { }
}
