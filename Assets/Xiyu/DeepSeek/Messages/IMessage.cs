namespace Xiyu.DeepSeek.Messages
{
    public interface IMessage
    {
        Role Role { get; }
        string Content { get; set; }

        ISerializer Serializer { get; }
    }
}