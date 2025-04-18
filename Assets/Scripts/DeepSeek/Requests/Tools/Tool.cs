namespace Xiyu.DeepSeek.Requests.Tools
{
    public class Tool<T> : ITool
    {
        public ToolType Type { get; set; } = ToolType.Function;

        public T Function { get; set; }
    }
}