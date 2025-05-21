using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Xiyu.DeepSeek.Cryptography;
using Xiyu.DeepSeek.Messages;
using Xiyu.DeepSeek.Persistence;
using Xiyu.DeepSeek.Requests;

namespace Xiyu
{
    public class SerializeTest : MonoBehaviour
    {
        private async void Start()
        {
            var messagesCollector = new MessagesCollector(
                new SystemMessage("系统消息"),
                new UserMessage("用户消息"),
                new AssistantMessage("助手消息"),
                new UserMessage("用户消息2"),
                new AssistantPrefixMessage("用户对话前缀续写"),
                new AssistantMessage("助手消息2")
            );

            // 获取用户桌面
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            var savePath = desktop + "/messages.txt";

            await messagesCollector.Messages
                .DoRemove()
                .DoSerialize()
                .DoEncryption(Environment.UserName)
                .AsFileAsync(savePath);

            var asUniTask = await File.ReadAllTextAsync(savePath).AsUniTask();
            var decrypt = CryptoExtensions.Decrypt(asUniTask, Environment.UserName);
            Debug.Log(decrypt);
        }
    }
}