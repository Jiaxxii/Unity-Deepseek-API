using Newtonsoft.Json.Linq;

namespace Xiyu.DeepSeek.Messages
{
    public interface ISerializer
    {
        JObject SerializeJson(IMessage message);
    }
}