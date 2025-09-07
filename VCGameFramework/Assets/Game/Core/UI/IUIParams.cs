namespace Game.Core.UI
{
    public interface IUIParams
    {
        
    }
    
    public struct EmptyUIParams : IUIParams
    {
        public static readonly EmptyUIParams Instance = new EmptyUIParams();
    }
}